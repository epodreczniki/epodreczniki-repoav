using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSNC.RepoAV.RepDBAccess;

namespace PSNC.RepoAV.RepDBAccessTests
{
	[TestClass]
	public class UnitTest1
	{
		static PSNC.RepoAV.RepDBAccess.RepDBAccess dba = new PSNC.RepoAV.RepDBAccess.RepDBAccess(@"Data Source=(LocalDB)\v11.0;Integrated Security=SSPI;Initial Catalog=RepDB", false);

		[TestMethod]
		public void GetTasksCountTest()
		{
			TaskCount[] tcs = dba.GetTasksCount(null, new TaskStatus[] {TaskStatus.Executing});

			Assert.IsTrue(tcs != null);
		}

		[TestMethod]
		public void GetFormatGroup()
		{
			FormatGroup tcs = dba.GetFormatGroup(3);

			Assert.IsTrue(tcs != null);
		}

		//[TestMethod]
		//public void DecimalTest()
		//{
		//	DecimalT dres = new DecimalT();

		//	DecimalTRes[] res = dba.GetDecimalTest(dres);


		//	Assert.IsTrue(res != null);
		//}

		[TestMethod]
		public void GetPublicIds4ChangedMaterialsSinceTest()
		{
			string[] ids = dba.GetPublicIds4ChangedMaterialsSince(DateTime.MinValue, DateTime.Now);

			Assert.IsTrue(ids != null);
		}
	}
}
