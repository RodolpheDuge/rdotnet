﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDotNet;
using RDotNet.Devices;

namespace StressTest
{
   class Program
   {
      private static readonly ICharacterDevice device = new ConsoleDevice();

      private static void Main(string[] args)
      {
         REngine.SetEnvironmentVariables();
         using (var engine = REngine.GetInstance(device: device))
         {
            engine.EnableLock = true;
            var r = new RuntimeDiagnostics(engine);
            int sizeEach = 20;
            int n = 1000;
            NumericVector[] nVecs = r.CreateNumericVectors(n, sizeEach);
            nVecs = r.Lapply(nVecs, "function(x) {x * 2}");

            // doMultiThreadingOperation(r, sizeEach, n);
            doTestObjectFinalization(r, sizeEach, n);
            doTestObjectFinalization(r, sizeEach, n, disposeSexp:true);
            nVecs = null;
            engine.Dispose(); // deliberate test 
         }
      }

      private static void doMultiThreadingOperation(RuntimeDiagnostics r, int sizeEach, int n)
      {
         var pVecs = new NumericVector[2][];
         Parallel.For(0, 2, i => pVecs[i] = r.CreateNumericVectors(n, sizeEach));
      }

      private static void doTestObjectFinalization(RuntimeDiagnostics r, int sizeEach, int n, bool disposeSexp=false)
      {
         NumericVector[] v = r.CreateNumericVectors(n, sizeEach);
         checkSizes(v, sizeEach);
         var v2 = r.Lapply(v, "function(x) {x * x}");
         checkSizes(v2, sizeEach);
         v = null;
         for (int i = 0; i < 5; i++)
         {
            GC.Collect();
            v = r.CreateNumericVectors(n, sizeEach);
            checkSizes(v, sizeEach);
            if (disposeSexp)
               for (int j = 0; j < v.Length; j++)
               {
                  v[j].Dispose();
               }
         }
      }

      private static void checkSizes(NumericVector[] v, int sizeEach)
      {
         for (int i = 0; i < v.Length; i++)
         {
            if (v[i].Length != sizeEach)
               throw new Exception(string.Format("Unexpected length {0} at index {1}", v[i].Length, i));
         }
      }
   }
}