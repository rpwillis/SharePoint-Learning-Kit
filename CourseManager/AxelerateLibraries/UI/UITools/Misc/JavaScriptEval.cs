using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.JScript;


namespace Axelerate.BusinessLayerUITools.Misc
{
    public class Evaluator
    {
        public static int EvalToInteger(string statement)
        {
            string s = EvalToString(statement);
            return int.Parse(s.ToString());
        }

        public static double EvalToDouble(string statement)
        {
            string s = EvalToString(statement);
            return double.Parse(s);
        }

        public static string EvalToString(string statement)
        {
            object o = EvalToObject(statement);
            return o.ToString();
        }

        public static object EvalToObject(string statement)
        {
            return EvalToObject(statement, null);
        }
        public static object EvalToObject(string statement, object Datasource)
        {
            //FieldInfo DtsField = _evaluatorType.GetField("DataItem");
            //DtsField.SetValue(_evaluator, Datasource);
            string condition = Axelerate.BusinessLayerUITools.Common.clsSharedMethods.SharedMethods.parseBOPropertiesString((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)Datasource, statement, ((Axelerate.BusinessLayerFrameWork.BLCore.BLBusinessBase)Datasource).GetType().AssemblyQualifiedName, "");
            return _evaluatorType.InvokeMember(
                        "Eval",
                        BindingFlags.InvokeMethod,
                        null,
                        _evaluator,
                        new object[] { condition }
                     );
        }

        static Evaluator()
        {
//            ICodeCompiler compiler;
            JScriptCodeProvider Provider = new JScriptCodeProvider();
            //compiler = new JScriptCodeProvider().CreateCompiler();

            CompilerParameters parameters;
            parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            CompilerResults results;
            results = Provider.CompileAssemblyFromSource(parameters, _jscriptSource);
           // results = compiler.CompileAssemblyFromSource(parameters, _jscriptSource);

            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Evaluator.Evaluator");

            _evaluator = Activator.CreateInstance(_evaluatorType);
        }

        private static object _evaluator = null;
        private static Type _evaluatorType = null;
        private static readonly string _jscriptSource =

            @"package Evaluator
            {
               class Evaluator
               {
                  public var DataItem: Object;

                  public function Eval(expr : String) : String 
                  { 
                     return eval(expr); 
                  }
               }
            }";
    }

}