using System;
using System.Runtime.Serialization;

namespace WTW.MdpService.Infrastructure.Consumers;

[Serializable]
public class MdpConsumerException : Exception, ISerializable
{
    public MdpConsumerException(string message) : base(message)
    {
    }

    protected MdpConsumerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        // Deserialize any custom properties here if needed
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        base.GetObjectData(info, context);
    }
}

