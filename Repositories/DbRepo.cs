using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Repositories
{
	public class DbRepo
	{
		private Dictionary<string, Task> dataEvents = new Dictionary<string, Task>();
		private bool assignedTransactionEvents = false;
		private List<string> executedActions = new List<string>();

		public int ActionsExecuted { get; set; } = 0;

		public void AddActionListener(string actionName, Task task)
		{
			dataEvents.Add(actionName, task);
		}

		public void ExecuteAction(string action)
		{
			ActionsExecuted++;
			executedActions.Add(action);
			Commit(action);
		}

		private void Commit(string action)
		{
			if (HasTransaction()) return; // actions will be triggered on TransactionComplete

			ExecuteDataEventListener(action);
		}

		public bool HasTransaction()
		{
			if (Transaction.Current == null)
				return false;

			if (assignedTransactionEvents)
				return true;

			assignedTransactionEvents = true;
			Transaction.Current.TransactionCompleted += Current_TransactionCompleted;

			return true;
		}

		private void Current_TransactionCompleted(object sender, TransactionEventArgs transactionEvent)
		{
			if (transactionEvent.Transaction.TransactionInformation.Status != TransactionStatus.Committed)
				return; // rolled back transaction

			// committed transaction - execute listeners
			foreach (var action in executedActions)
			{
				ExecuteDataEventListener(action);
			}
		}

		private void ExecuteDataEventListener(string action)
		{
			var task = dataEvents.SingleOrDefault(e => e.Key == action).Value;
			task.RunSynchronously();
		}
	}
}
