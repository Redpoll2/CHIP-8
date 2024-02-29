using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ChipEight.Runtime
{
	public class ChipEightVirtualMachine : IAsyncDisposable, IDisposable
	{
		private readonly ChipEightInterpreter _interpreter;
		private MemoryStream? _memory;
		private bool _disposed;

		public ChipEightVirtualMachine(Stream? binary = null)
		{
			_memory = new MemoryStream(0x1000)
			{
				Position = 0x200
			};

			if (binary is not null)
			{
				Debug.WriteLineIf(binary.Length > 0x1000, "Binary stream is bigger than 4096 bytes. The stream will be truncated.");

				binary.CopyTo(_memory);

				// go back to the beggining of the instructions.
				_memory.Position = 0x200;
			}

			_interpreter = new ChipEightInterpreter(this);
		}

		public MemoryStream Memory => _memory ?? throw new ObjectDisposedException(GetType().Name);

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_memory?.Dispose();
				}

				_memory = null;
				_disposed = true;
			}
		}

		protected virtual async ValueTask DisposeAsyncCore()
		{
			if (!_disposed)
			{
				if (_memory is not null)
					await _memory.DisposeAsync().ConfigureAwait(false);
			}
		}

		public void Run()
		{
			while (true)
			{
				_interpreter.Step();
			}
		}

		public static void Main(string[]? args)
		{
			string binaryPath = @"D:\\Maze (alt) [David Winter, 199x].ch8";

			using var binaryStream = new FileStream(binaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var virtualMachine = new ChipEightVirtualMachine(binaryStream);

			virtualMachine.Run();
		}

		~ChipEightVirtualMachine()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore();

			Dispose(disposing: false);
			GC.SuppressFinalize(this);
		}
	}
}
