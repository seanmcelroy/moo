using System.Threading.Tasks;
using moo.common.Models;
using NUnit.Framework;

namespace Tests
{
    public class LockTest
    {
        [Test]
        public void ParseValid()
        {
            Assert.IsTrue(Lock.TryParse("sex:female", out _));
            Assert.IsTrue(Lock.TryParse("sex:*ale", out _));
            Assert.IsTrue(Lock.TryParse("*passiflora|*kenya", out _));
            Assert.IsTrue(Lock.TryParse("~staff:yes&sex:female", out _));
            Assert.IsTrue(Lock.TryParse("!*stinker", out _));
            Assert.IsTrue(Lock.TryParse("*jessy&!*jessy", out _));
            Assert.IsTrue(Lock.TryParse("!*stinker|sex:female", out _));
            Assert.IsTrue(Lock.TryParse("!(*stinker|sex:female)", out _));
        }
    }
}