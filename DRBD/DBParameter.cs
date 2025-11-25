
namespace DRBD
{
    public class DBParameter(string name, object value)
    {
        public string Name { get; set; } = name;
        public object Value { get; set; } = value;
    }
}
