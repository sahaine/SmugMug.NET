// Copyright (c) Alex Ghiondea. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace SmugMugShared.Extensions
{
   public static class ConsolePrinter
   {
      public static void Write((ConsoleColor, string) consoleResult)
      {
         Write(consoleResult.Item1, consoleResult.Item2);
      }

      public static void Write(ConsoleColor color, string message, params object[] args)
      {
         var curColor = Console.ForegroundColor;
         Console.ForegroundColor = color;

         Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}", args);

         Console.ForegroundColor = curColor;
      }

      const char _block = '■';
      const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
      const string _twirl = "-\\|/";
      public static void WriteProgressBar(int percent, bool update = false)
      {
         if (update)
            Console.Write(_back);
         Console.Write("[");
         var p = (int)((percent / 10f) + .5f);
         for (var i = 0; i < 10; ++i)
         {
            if (i >= p)
               Console.Write(' ');
            else
               Console.Write(_block);
         }
         Console.Write("] {0,3:##0}%", percent);
      }

      public static void WriteProgress(int progress, bool update = false)
      {
         if (update)
            Console.Write("\b");
         Console.Write(_twirl[progress % _twirl.Length]);
      }

   }

   public class ConsoleSpinner : IDisposable
   {
      private const string Sequence = @"/-\|";
      private readonly int _delay;
      private readonly DateTime _start;
      private bool _active = true;
      private int _counter = 0;

      public ConsoleSpinner(int delay = 100)
      {
         _delay = delay;
         _start = DateTime.UtcNow;
         Console.Write(" ");
         Task.Run(Spin);
      }

      public void Stop()
      {
         _active = false;
         Console.Write("\b");
         Console.CursorVisible = true;
         ConsolePrinter.Write(ConsoleColor.White, $"Took {DateTime.UtcNow - _start}");
      }

      private void Spin()
      {
         Console.CursorVisible = false;
         while (_active)
         {
            Turn();
            Task.Delay(_delay).Wait(); 
         }
      }

      private void Draw(char c)
      {
         
         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write("\b");
         Console.Write(c);
      }

      private void Turn()
      {
         Draw(Sequence[++_counter % Sequence.Length]);
      }

      public void Dispose()
      {         
         Stop();
      }
   }
}
