using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace ChipEight.Runtime
{
	public class ChipEightInterpreter
	{
		private readonly MemoryStream _memory;
		private readonly byte[] _registers;
		private readonly Stack<short> _stack;
		private readonly unsafe short* _index;

		public ChipEightInterpreter(ChipEightVirtualMachine virtualMachine)
		{
			if (virtualMachine is null)
				throw new ArgumentNullException(nameof(virtualMachine));

			_memory = virtualMachine.Memory;

			_registers = new byte[0xF];
			_stack = new Stack<short>(0xF);
		}

		public void Step()
		{
			byte low = (byte)_memory.ReadByte();
			byte high = (byte)_memory.ReadByte();

			// if opcode has not operands
			if (low == 0x00)
			{
				switch (high)
				{
					case 0xE0:	// CLS
						ClearScreen();
						return;

					case 0xEE:	// RET
						Return();
						return;
				}
			}

			byte x = (byte)(low & 0xF);
			byte y = (byte)(high >> 4);
			short address = (short)((x << 8) | high);

			byte nibble = (byte)(low >> 4);

			switch (nibble)
			{
				case 0x0:
					SystemCall(address);
					return;

				case 0x1:
					Jump(address);
					return;

				case 0x2:
					Call(address);
					return;

				case 0x3:
					SkipIfEquals(x, high);
					return;

				case 0x4:
					SkipIfNotEquals(x, high);
					return;

				case 0x5:
					SkipIfRegistersEquals(x, y);
					return;

				case 0x6:
					SetRegisterValue(x, high);
					return;

				case 0x7:
					IncrementRegister(x, high);
					return;
			}

			Debug.WriteLine("Invalid opcode received: 0x" + low.ToString("X2") + high.ToString("X2"));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		private void SystemCall(short address)
		{
			// dunno. is it useful for the emulator?
			Debug.WriteLine("SYS " + address.ToString("X3"));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ClearScreen()
		{
			// ask renderer to clear buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Return()
		{
			if (!_stack.TryPop(out short address))
				address = 0x1000; // beginning of the program

			_memory.Position = address;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Jump(short address)
		{
			_memory.Position = address;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Call(short address)
		{
			_stack.Push((short)_memory.Position);
			_memory.Position = address;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipIfEquals(byte index, byte value)
		{
			if (_registers[index] == value)
				_memory.Position += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipIfNotEquals(byte index, byte value)
		{
			if (_registers[index] != value)
				_memory.Position += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipIfRegistersEquals(byte left, byte right)
		{
			if (_registers[left] == _registers[right])
				_memory.Position += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetRegisterValue(byte index, byte value)
		{
			_registers[index] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncrementRegister(byte index, byte value)
		{
			_registers[index] += value;
		}



		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipIfRegistersNotEquals(byte left, byte right)
		{
			if (_registers[left] != _registers[right])
				_memory.Position += 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void SetMemoryIndex(short address)
		{
			*_index = address;
		}
	}
}
