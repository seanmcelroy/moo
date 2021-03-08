using moo.common.Models;

namespace moo.common.Database
{
    public struct RepositoryGetResult<T> where T : Thing
    {
        public T? value;

        public bool isSuccess;

        public string reason;

        public RepositoryGetResult(T? value, string reason)
        {
            this.value = value;
            this.isSuccess = true;
            this.reason = reason;
        }

        public RepositoryGetResult(string reason)
        {
            this.value = null;
            this.isSuccess = false;
            this.reason = reason;
        }
    }
}