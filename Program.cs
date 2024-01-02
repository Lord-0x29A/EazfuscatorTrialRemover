using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using System;
using System.Linq;

namespace EazTrialRemover2024
{
    internal class Program
    {
        private static ModuleDefMD _module;
        private static bool _DLL;
        static void Main(string[] args)
        {
            _module = ModuleDefMD.Load(args[0], ModuleDefMD.CreateModuleContext());
            var output = args[0].Insert(args[0].Length - 4, "_TrialRemoved");
         
            if (output.EndsWith(".dll") || output.EndsWith(".DLL"))
                _DLL = true;

            FindAndRemoveTrial();

            var opts = new ModuleWriterOptions
                 (_module);  
            opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            _module.Write(output, opts);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        private static void FindAndRemoveTrial()
        {
            if(_DLL)
                _module.GlobalType.FindStaticConstructor().Body.Instructions[1].OpCode = OpCodes.Nop;
         
            bool removed = false;
          
           foreach(var t in _module.GetTypes().ToList())
           {
               if (t.NestedTypes.Count != 2) continue;
               foreach(var m in t.Methods.ToList())
               {
                 if(m.ReturnType != _module.CorLibTypes.Boolean) continue;
                 if(m.Parameters.Count != 0) continue;
                 if(m.Body.Instructions.Count != 3) continue;    
               
                 Console.WriteLine($"Trial removed: 0x{m.MDToken:x}");
                 m.Body.Instructions.Clear();
                 m.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
                 m.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                 removed = true;       
               }
           }
       
           if (!removed)
           {
                foreach (var t in _module.GetTypes().ToList())
                {
                    if (t.Methods.Count != 4) continue;
                    foreach (var m in t.Methods.ToList())
                    {
                        if (m.ReturnType != _module.CorLibTypes.Boolean) continue;
                        if (m.Parameters.Count != 0) continue;
                        if (m.Body.Instructions.Count != 3) continue;
                      
                        Console.WriteLine($"Trial removed: 0x{m.MDToken:x}");
                        m.Body.Instructions.Clear();
                        m.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4_1));
                        m.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                        removed = true;
                    }
                }
            }

            if (!removed)
                Console.WriteLine("Can't remove trial");
        }
    }
}
