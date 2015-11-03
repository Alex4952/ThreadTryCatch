using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadTryCatch
{

	class UserInput
	{
		internal static Queue<string> commandQueue = new Queue<string>();
		internal ICollection iCollection = commandQueue;

		internal void CatchInput()
		{
			while (true)
			{
				string input = Console.ReadLine();

				lock (iCollection.SyncRoot)
				{
					Console.WriteLine("+++++ START LOCK for INPUT +++++");
					commandQueue.Enqueue(input);
					if (string.Compare("q", input, true) == 0)
					{
						Console.WriteLine("+++++ Q-Break. END LOCK for INPUT +++++ ");
						Program.autoResetEvent.Set();
						break;
					}
					Thread.Sleep(3000);
					Console.WriteLine("+++++ END LOCK for INPUT +++++");
					Program.autoResetEvent.Set();
				}
				Thread.Yield();
			}

			Console.WriteLine("Input Thread is finished.");
		}
	}

	class Handler
	{
		internal void DoAction()
		{
			bool bStop = false;
			Thread.Sleep(5000);
			Queue<string> handlerQueue = new Queue<string>();

			while (!bStop)
			{
				// commandQueue에 명령이 있으면 동작, command가 q이면 break
				Program.autoResetEvent.WaitOne();
				//UserInput.mySema.Acquire();

				lock (((ICollection)UserInput.commandQueue).SyncRoot)
				{
					Console.WriteLine("+++++ START LOCK for HANDLER +++++");
					int cmdCount = UserInput.commandQueue.Count;
					Console.WriteLine("카피할 명령수 cmdCount: {0}", cmdCount);
					// 핸들러의 큐에 카피
					for (int i = 0; i < cmdCount; i++)
					{
						string command = UserInput.commandQueue.Dequeue();
						Console.WriteLine("카피 command: {0}", command);
						handlerQueue.Enqueue(command);
					}
					Thread.Sleep(3000);
					Console.WriteLine("+++++ END LOCK for HANDLER +++++");
				}

				int handlerCmdCount = handlerQueue.Count;
				// 명령 처리
				for (int i = 0; i < handlerCmdCount; i++)
				{
					string command = handlerQueue.Dequeue();
					if (string.Compare("q", command, true) == 0)
					{
						bStop = true;
						break;
					}

					Console.WriteLine("{0} 명령 처리중..완료.", command);
				}
			}

			Console.WriteLine("Handler Thread is finished.");
		}
	}

	class Program
	{
		internal static AutoResetEvent autoResetEvent;

		static void Main(string[] args)
		{
			autoResetEvent = new AutoResetEvent(false); // true: 초기값을 신호받음 상태로.
														
			// 사용자 입력 스레드
			UserInput userInput = new UserInput();
			Thread userInputThread = new Thread(userInput.CatchInput);
			userInputThread.Start();

			// 명령 수행 스레드
			Handler handler = new Handler();
			Thread handlerThread = new Thread(handler.DoAction);
			handlerThread.Start();

			// 종료시 처리할 일
			userInputThread.Join();
			handlerThread.Join();

			Console.WriteLine("Main Thread is finished.");
		}
	}
}
