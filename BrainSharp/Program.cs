using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace BrainSharp
{
    internal class Program
    {
        private static ILGenerator ilg;
        private static LocalBuilder v3;
        
        private static Stack<Label> labels;
        private static Stack<Label> labels2;

        private static void Main(string[] args)
        {            
            labels = new Stack<Label>();
            labels2 = new Stack<Label>();

            AssemblyName name = new AssemblyName();
            name.Name = "Test";
            AssemblyBuilder bld = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);

            ModuleBuilder mb = bld.DefineDynamicModule(name.Name, "Hello.exe");

            TypeBuilder tb = mb.DefineType("Program", TypeAttributes.Public | TypeAttributes.Class);
            MethodBuilder fb = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[] { typeof(string[]) });

            ilg = fb.GetILGenerator();

            ilg.DeclareLocal(typeof(byte[]));
            ilg.DeclareLocal(typeof(Int32));
            ilg.DeclareLocal(typeof(bool));
            v3 = ilg.DeclareLocal(typeof(ConsoleKeyInfo));

            ilg.Emit(OpCodes.Ldc_I4, 30000);
            ilg.Emit(OpCodes.Newarr, typeof(byte));
            ilg.Emit(OpCodes.Stloc_0);

            char[] code;
            int cp = 0;

            if (args.Length == 0)
                code = Console.ReadLine().ToCharArray();
            else
                code = File.ReadAllText(args[0]).ToCharArray();

            while (cp < code.Length)
            {
                char c = code[cp];

                switch (c)
                {
                    case '<': EmitMpMinus(); break;
                    case '>': EmitMpPlus(); break;
                    case '+': EmitMemPlus(); break;
                    case '-': EmitMemMinus(); break;
                    case '.': EmitWrite(); break;
                    case ',': EmitRead(); break;
                    case '[': EmitLoopStart(); break;
                    case ']': EmitLoopEnd(); break;
                }

                cp++;
            }

            ilg.Emit(OpCodes.Ret);

            Type t = tb.CreateType();
            // Set the entrypoint (thereby declaring it an EXE)
            bld.SetEntryPoint(fb, PEFileKinds.ConsoleApplication);

            // Save it
            bld.Save("Hello.exe");
        }

        private static void EmitLoopStart()
        {
            var br = ilg.DefineLabel();
            ilg.Emit(OpCodes.Br, br);
            labels.Push(br);

            var br2 = ilg.DefineLabel();
            ilg.MarkLabel(br2);
            labels2.Push(br2);
        }

        private static void EmitLoopEnd()
        {
            var lcc = labels.Pop();
            ilg.MarkLabel(lcc);
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldelem_U1);
            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Cgt_Un);
            ilg.Emit(OpCodes.Stloc_2);
            ilg.Emit(OpCodes.Ldloc_2);
            ilg.Emit(OpCodes.Brtrue, labels2.Pop());
        }

        private static void EmitRead()
        {
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadKey", new Type[] { }));
            ilg.Emit(OpCodes.Stloc_3);
            ilg.Emit(OpCodes.Ldloca_S, v3);
            ilg.Emit(OpCodes.Call, typeof(ConsoleKeyInfo).GetMethod("get_KeyChar"));
            ilg.Emit(OpCodes.Conv_U1);
            ilg.Emit(OpCodes.Stelem_I1);
        }

        private static void EmitWrite()
        {
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldelem_U1);
            ilg.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(char) }));
        }

        private static void EmitMemMinus()
        {
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldelema, typeof(byte));
            ilg.Emit(OpCodes.Dup);
            ilg.Emit(OpCodes.Ldind_U1);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Sub);
            ilg.Emit(OpCodes.Conv_U1);
            ilg.Emit(OpCodes.Stind_I1);
        }

        private static void EmitMemPlus()
        {
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldelema, typeof(byte));
            ilg.Emit(OpCodes.Dup);
            ilg.Emit(OpCodes.Ldind_U1);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Conv_U1);
            ilg.Emit(OpCodes.Stind_I1);
        }

        private static void EmitMpPlus()
        {
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Add);
            ilg.Emit(OpCodes.Stloc_1);
        }

        private static void EmitMpMinus()
        {
            ilg.Emit(OpCodes.Ldloc_1);
            ilg.Emit(OpCodes.Ldc_I4_1);
            ilg.Emit(OpCodes.Sub);
            ilg.Emit(OpCodes.Stloc_1);
        }
    }
}