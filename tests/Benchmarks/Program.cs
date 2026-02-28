using BenchmarkDotNet.Running;
using System.Reflection;

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
