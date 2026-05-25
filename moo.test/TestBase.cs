using Microsoft.VisualStudio.TestTools.UnitTesting;
using moo.common;

namespace moo.Test
{
    public abstract class TestBase
    {
        [TestInitialize]
        public void BaseSetup()
        {
            ThingRepository.Instance.Clear();
        }
    }
}
