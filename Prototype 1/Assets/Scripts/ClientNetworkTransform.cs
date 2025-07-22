using Unity.Netcode.Components;

public class ClientNetworkTransform : NetworkTransform
{
    // This class can be used to extend the functionality of NetworkTransform
    // for client-specific behavior, if needed.

    // For example, you could override methods to add custom synchronization logic
    // or to handle specific client-side transformations.

    // Currently, it inherits all functionality from NetworkTransform.
    protected override bool OnIsServerAuthoritative()
    {
        // This method can be overridden if you want to change the server authority behavior.
        return false;
    }
}