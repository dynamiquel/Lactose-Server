using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace LactoseWebApp.Mapping;

public class ProtobufMapper
{
    protected static Timestamp DateTimeToTimestamp(DateTime dateTime) => Timestamp.FromDateTime(dateTime);
    protected static RepeatedField<T> EnumerableToRepeatedField<T>(IEnumerable<T> enumerable) => new() { enumerable };
}