using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Tests
{
	[TestClass]
	public class TransactionTests
	{
		[TestMethod]
		public void HasTransactionTest()
		{
			var repo = new DbRepo();

			using (var tranScope = new TransactionScope())
			{
				Assert.IsTrue(repo.HasTransaction());
			}

			Assert.IsFalse(repo.HasTransaction());
		}

		[TestMethod]
		public void NoTransactionTest()
		{
			var repo = new DbRepo();
			Assert.IsFalse(repo.HasTransaction());
		}

		[TestMethod]
		public void EventListenerTest()
		{
			var repo = new DbRepo();
			var actionName = "foo";

			var task = new Task<bool>(() =>
			{
				return repo.HasTransaction();
			});
			
			repo.AddActionListener(actionName, task);
			repo.ExecuteAction(actionName);
			Assert.IsTrue(task.IsCompleted);
			Assert.IsFalse(task.Result);
		}

		[TestMethod]
		public void TransactionEventListenerTest()
		{
			var repo = new DbRepo();
			var actionName = "foo";

			var task = new Task<bool>(() =>
			{
				return repo.HasTransaction();
			});

			repo.AddActionListener(actionName, task);

			using (var tranScope = new TransactionScope())
			{
				repo.ExecuteAction(actionName);
				Assert.IsFalse(task.IsCompleted);

				tranScope.Complete();
			}
			
			Assert.IsTrue(task.IsCompleted);
		}
	}
}
