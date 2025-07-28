using Microsoft.AspNetCore.Components.Server.Circuits;

namespace SyncArea.Identity
{
    public class MyCircuit : CircuitHandler
    {
        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }
    }
}
