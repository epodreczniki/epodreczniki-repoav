using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace PSNC.Proca3.Subsystem
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public class DynamicClientProxy<TInterface> where TInterface : class
    {
        #region static Fields

        /// <summary>
        /// This will contain the client proxies which are already created.
        /// </summary>
        private static Dictionary<string, TInterface> cachedClientProxies;

        private static ClientProxyClassBuilder<TInterface> classBuilder;
        
        #endregion


        #region Fields

        /// <summary>
        /// This is the "real" proxy used to interface with the WCF service. 
        /// However when an error occurs with the connection this proxy can no 
        /// longer be used and it should be recreated.
        /// </summary>
        private InternalProxy<TInterface> _clientProxy = null;

        /// <summary>
        /// The binding that should be used to communicate with the remote WCF 
        /// service.
        /// </summary>
        private Binding currentBinding = null;

        /// <summary>
        /// The end point address of the WCF service.
        /// </summary>
        private EndpointAddress cuurentEndpointAddress = null;

        #endregion

        #region Classes
        class InternalProxy<TInterface> : ClientBase<TInterface> where TInterface : class
        {
            public InternalProxy(Binding binding, EndpointAddress address)
                : base(binding, address)
            {
                this.Endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
            }
        }
        #endregion

        #region Constructor

        static DynamicClientProxy()
        {
            cachedClientProxies = new Dictionary<string, TInterface>();
            classBuilder = new ClientProxyClassBuilder<TInterface>();
        }


        protected DynamicClientProxy(Binding binding, EndpointAddress endpointAddress)
        {
            this.currentBinding = binding;
            this.cuurentEndpointAddress = endpointAddress;

            CreateClientProxy();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Make the channel of the enclosed proxy available for the derived and 
        /// generated proxy implementations. 
        /// </summary>
        protected TInterface Channel
        {
            get { return _clientProxy.InnerChannel as TInterface; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpointAddress"></param>
        /// <returns></returns>
        public static TInterface GetInstance(EndpointAddress endpointAddress)
        {
            TInterface instance = null;

            Binding binding = new WebHttpBinding();
            string identifier = endpointAddress.ToString();

            if (cachedClientProxies.TryGetValue(identifier, out instance) == false)
            {
                // The interface is not yet present in the cache

                string typeIdentifier = typeof(TInterface).Name;

                // Prepare new type name - remove the 'I' from the type name. 
                if (typeIdentifier.ToUpper().StartsWith("I"))
                {
                    typeIdentifier = typeIdentifier.Substring(1);
                }

                // create new type
                Type type = classBuilder.GenerateType(typeIdentifier);

                // Create an instance of the proxy and construct it with the 
                // passed binding and endpoint address.
                instance = (TInterface)Activator.CreateInstance(type, new object[] { binding, endpointAddress });

                // add instance to the cache
                cachedClientProxies.Add(identifier, instance);
            }

            return instance;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Close the current client proxy.
        /// </summary>
        private void CloseClientProxy()
        {
            if (_clientProxy != null)
            {
                if (_clientProxy.InnerChannel.State != CommunicationState.Faulted)
                {
                    _clientProxy.Close();
                }
                else
                {
                    _clientProxy.Abort();
                }

                _clientProxy = null;
            }
        }

        /// <summary>
        /// Create the actual client proxy based upon the passed binding and 
        /// address of the service.
        /// </summary>
        private void CreateClientProxy()
        {
            if (_clientProxy != null)
            {
                CloseClientProxy();
            }

            _clientProxy = new InternalProxy<TInterface>(this.currentBinding, this.cuurentEndpointAddress);
        }

        /// <summary>
        /// Handle the passed exception which occurred while calling a 
        /// method/property on the service.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        protected virtual void HandleException(Exception exception)
        {
            // Recreate the client proxy.
            CreateClientProxy();

            // We need to rethrow the exception wrappen in our 
            // ConnectionProblemException so that callers of the methods/properties 
            // of the service know that this exception needs to be catched.
            if ((exception is EndpointNotFoundException) || (exception is CommunicationException))
            {
                throw new ApplicationException("Server unreachable", exception);
            }
            else
            {
                throw new ApplicationException("Unknown exception", exception);
            }
        }

        #endregion

     
    }
}
