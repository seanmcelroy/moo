public interface IStorable<T> where T : Thing, new() {
    string Serialize();
}