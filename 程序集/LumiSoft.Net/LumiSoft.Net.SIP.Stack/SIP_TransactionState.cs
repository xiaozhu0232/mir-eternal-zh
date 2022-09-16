namespace LumiSoft.Net.SIP.Stack;

public enum SIP_TransactionState
{
	WaitingToStart,
	Calling,
	Trying,
	Proceeding,
	Accpeted,
	Completed,
	Confirmed,
	Terminated,
	Disposed
}
