using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace PSNC.Proca3.Subsystem
{
    // Przykład użycia:
    //  string baseAddress = "http://localhost:8088/Recoder";
    //  IRecoderSubsytem pxy = DynamicClientProxy<IRecoderSubsytem>.GetInstance(new EndpointAddress(baseAddress));
    //  string name = pxy.GetName();


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    internal class ClientProxyClassBuilder<TInterface> where TInterface : class
    {
        private const string PROXY_ASSEMBLY_NAME = "DynamicProxyClassContainer";

        #region Fields

        /// <summary>
        /// This is used to store the generated types in so that future retrievals 
        /// can retrieve it from this cache.
        /// </summary>
        private Dictionary<string, Type> cachedTypes = new Dictionary<string, Type>();

        /// <summary>
        /// The default attributes that we use for the generated methods.
        /// </summary>
        private const MethodAttributes DEFAULT_METHOD_ATTRIBUTES =
            MethodAttributes.Public | MethodAttributes.Virtual |
            MethodAttributes.Final | MethodAttributes.NewSlot;

        /// <summary>
        /// The builder that we use to generate the assembly which will contain 
        /// the generated proxy classes.
        /// </summary>
        private AssemblyBuilder assemblyBuilder = null;

        /// <summary>
        /// The builder that we use to generate the module which will contain the 
        /// generated proxy classes.
        /// </summary>
        private ModuleBuilder moduleBuilder = null;

       #endregion


        #region Constructor

        public ClientProxyClassBuilder()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate a type based on the passed class name within the generated 
        /// assembly. When a type is generated it will be cached so that future 
        /// calls do not get the penalty for creating it.
        /// </summary>
        /// <param name="className">The class name for which a type needs to be generated.</param>
        /// <returns></returns>
        public Type GenerateType(string className)
        {
            Type type = null;

            // We first need to check if we already created a type for the 
            // passed class.
            if (!cachedTypes.TryGetValue(className, out type))
            {
                GenerateAssembly();

                if (moduleBuilder != null)
                {
                    type = moduleBuilder.GetType(className);

                    // This type is not yet generated, so let's do that.
                    if (type == null)
                    {
                        type = GenerateTypeImplementation(className);

                        if (type != null)
                        {
                            cachedTypes.Add(className, type);
                        }
                    }
                }
            }

            return type;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Generate the assembly that is used for containing the generated 
        /// proxy classes. 
        /// </summary>
		private void GenerateAssembly()
        {
            if (assemblyBuilder == null)
            {
                AssemblyName assemblyName = new AssemblyName(PROXY_ASSEMBLY_NAME);

                assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            }
        }

        /// <summary>
        /// Generate the implementation for the type of the class identified 
        /// by the passed class name.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>The generated type.</returns>
        private Type GenerateTypeImplementation(string className)
        {
            Type generatedType = null;

            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                PROXY_ASSEMBLY_NAME + "." + className,
                TypeAttributes.Public, typeof(DynamicClientProxy<TInterface>));

            if (typeBuilder != null)
            {
                // Our generated type will implement the interface as passed 
                // via the generic principle.
                typeBuilder.AddInterfaceImplementation(typeof(TInterface));

                // We need to generate the constructor which enables the user(s) 
                // to define the binding and the address of the service.
                GenerateConstructor(typeBuilder);
                
                GenerateMethodImplementations(typeBuilder, typeof(TInterface));

                generatedType = typeBuilder.CreateType();
            }

            return generatedType;
        }

        /// <summary>
        /// Generate a constructor for the type we are currently generating. 
        /// </summary>
        /// <param name="typeBuilder">The builder that is used for generating 
        /// the new type definition.</param>
        private void GenerateConstructor(TypeBuilder typeBuilder)
        {
            // Define the constructor, it has 2 parameters; namely the binding 
            // used and the address of the service.
            Type[] constructorParameters = new[] { typeof(Binding), typeof(EndpointAddress) };

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.RTSpecialName,
                CallingConventions.Standard, constructorParameters);

            ILGenerator iLGenerator = constructorBuilder.GetILGenerator();

            // This.
            iLGenerator.Emit(OpCodes.Ldarg_0);
            // Load the first parameter (Binding).
            iLGenerator.Emit(OpCodes.Ldarg_1);
            // Load the second parameter (EndpointAddress).
            iLGenerator.Emit(OpCodes.Ldarg_2);

            // We will be calling the constructor from our base class. This 
            // generated class is deriving from the ClientProxy class.
            ConstructorInfo originalConstructor = 
                typeof(DynamicClientProxy<TInterface>).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic, 
                    null, constructorParameters, null);

            // Call the constructor from the base class.
            iLGenerator.Emit(OpCodes.Call, originalConstructor);

            // This method is finished.
            iLGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Save the current state of the assembly into a dll file. This makes 
        /// only sense when the ProxyClassBuilder is initialized with saveAssembly 
        /// set to true.
        /// </summary>

        /// <summary>
        /// Get the method from the base class with the matching method name 
        /// and return it.
        /// </summary>
        /// <param name="methodName">The name of the method</param>
        /// <returns>The method info of the matching method.</returns>
        /// <exception cref="InvalidOperationException">This exception is thrown 
        /// when no matching method could be found. This is a serious problem 
        /// since we are unable to generate a working proxy class.</exception>
        protected MethodInfo GetMethodFromBaseClass(string methodName)
        {
            MethodInfo methodInfo = typeof(DynamicClientProxy<TInterface>).GetMethod(methodName, 
                BindingFlags.Instance | BindingFlags.NonPublic | 
                BindingFlags.Public | BindingFlags.GetProperty); 

            if (methodInfo == null)
            {
                throw new InvalidOperationException(String.Format(
                    "The method {0} could not be found in {1}", 
                    methodName, typeof(DynamicClientProxy<TInterface>)));
            }

            return methodInfo;
        }

        /// <summary>
        /// Generate the method implementations in our generate type based upon 
        /// the passed type. This enables us to put an extra layer between the 
        /// user(s) and the actual proxy used to communicate with a service. The 
        /// extra layer is our ClientProxy that will extend every call to the 
        /// service with exception handling and reconnection behaviour.
        /// </summary>
        /// <param name="typeBuilder">The builder that is used to generate the 
        /// implementation for the type.</param>
        /// <param name="interfaceType">The interface for which we are generating 
        /// the methods</param>
        protected virtual void GenerateMethodImplementations(
            TypeBuilder typeBuilder, Type interfaceType)
        {
            MethodInfo[] methods = interfaceType.GetMethods();

            // Let's construct the methods as defined in the interface type so 
            // that we are able to generate our own implementation.
            foreach (MethodInfo method in methods)
            {
                // We need the types of the parameters for this method.
                Type[] parameterTypes = GetTypeOfParameters(method.GetParameters());

                MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name,
                    DEFAULT_METHOD_ATTRIBUTES, method.ReturnType, parameterTypes);

                // Start building the method.
                methodBuilder.CreateMethodBody(null, 0);
                ILGenerator iLGenerator = methodBuilder.GetILGenerator();

                // Generate the actual implementation for this method.
                GenerateMethodImplementation(method, parameterTypes, iLGenerator);

                // Declare that we override the interface method.
                typeBuilder.DefineMethodOverride(methodBuilder, method);
            }

            // Generate the implementations for any inherited interfaces.
            Type[] inheritedInterfaces = interfaceType.GetInterfaces();

            foreach (Type inheritedInterface in inheritedInterfaces)
            {
                GenerateMethodImplementations(typeBuilder, inheritedInterface);
            }
        }

        /// <summary>
        /// Generate the implementation for the method. It will generate the 
        /// functionality to call the method on the channel as offered by the 
        /// baseclass. It will add exception handling so that any exceptions 
        /// that occur (for example when no valid connection with the service is 
        /// present) are handled by the base class. The base class can then 
        /// take action to try to reconnect with the service.
        /// </summary>
        /// <param name="method">The method for which the implementation needs 
        /// to be generated.</param>
        /// <param name="parameterTypes">The parameter types which are passed 
        /// onto this method.</param>
        /// <param name="iLGenerator">The generator which needs to be used to 
        /// generate the implementation for the method.</param>
        protected void GenerateMethodImplementation(MethodInfo method, 
            Type[] parameterTypes, ILGenerator iLGenerator)
        {
            if (IsMethodWithReturnValue(method))
            {
                // Declare a variable which will contain the return type.
                iLGenerator.DeclareLocal(method.ReturnType);
            }

            // Add the 'try {' to the generated method implementation.
            Label tryLabel = iLGenerator.BeginExceptionBlock();
            {
                // This.
                iLGenerator.Emit(OpCodes.Ldarg_0); 
                
                // Get the Channel property from our base class. This enables 
                // us to call the 'real' implementation which will transfer the 
                // call onto the connected service.
                MethodInfo channelProperty = GetMethodFromBaseClass("get_Channel");
                // Get the channel: "base.Channel<TInterface>."
                iLGenerator.EmitCall(OpCodes.Call, channelProperty, null);

                // Prepare the parameters for the call.
                ParameterInfo[] parameters = method.GetParameters();

                for (int index = 0; index < parameterTypes.Length; index++)
                {
                    iLGenerator.Emit(OpCodes.Ldarg, (((short)index) + 1));
                }

                // Call the Channel via the interface.
                iLGenerator.Emit(OpCodes.Callvirt, method);

                if (IsMethodWithReturnValue(method))
                {
                    // returnValue = result of the function call.
                    iLGenerator.Emit(OpCodes.Stloc_0);
                }
            }
            // Add the 'catch (Exception)' to the generated method implementation.
            {
                iLGenerator.BeginCatchBlock(typeof(Exception));

                // Declare a local variable which will be used to store the 
                // received exception.
                int localVariableIndex = iLGenerator.DeclareLocal(typeof(Exception)).LocalIndex;

                // Get the exception from the stack.
                iLGenerator.Emit(OpCodes.Stloc_S, localVariableIndex);

                // This.
                iLGenerator.Emit(OpCodes.Ldarg_0);

                // Load the exception into the local variable.
                iLGenerator.Emit(OpCodes.Ldloc_S, localVariableIndex);

                // Let's call the method from our base class responsible for 
                // handling the exception. 
                MethodInfo handleExceptionMethod = 
                    GetMethodFromBaseClass("HandleException");

                // Let's call the virtual method.
                iLGenerator.Emit(OpCodes.Callvirt, handleExceptionMethod);
            }

            // Let's end the try/catch block.
            iLGenerator.EndExceptionBlock();

            if (IsMethodWithReturnValue(method))
            {
                // Return the returnValue.
                iLGenerator.Emit(OpCodes.Ldloc_0);
            }

            // This method is finished.
            iLGenerator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Get the types of the parameters passed.
        /// </summary>
        /// <param name="parameters">The parameters for which we should return 
        /// the types.</param>
        /// <returns>The types for the passed parameters</returns>
        private static Type[] GetTypeOfParameters(ParameterInfo[] parameters)
        {
            List<Type> typeOfParameters = new List<Type>();

            foreach (ParameterInfo parameter in parameters)
            {
                typeOfParameters.Add(parameter.ParameterType);
            }

            return typeOfParameters.ToArray();
        }

        /// <summary>
        /// This checks if the passed method returns a value or not.
        /// </summary>
        /// <param name="methodInfo">The method that needs to be checked.</param>
        /// <returns>True when the method returns a value, false when it is a 
        /// void method.</returns>
        private bool IsMethodWithReturnValue(MethodInfo methodInfo)
        {
            return (methodInfo.ReturnType != typeof(void));
        }

        #endregion

    }
}
