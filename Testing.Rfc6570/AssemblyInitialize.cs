using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing.Rfc6570
{
  [TestClass]
  public class AssemblyInitialize
  {
    [AssemblyInitialize]
    public static void AssemblyInitializer(TestContext testContext)
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }
  }
}
