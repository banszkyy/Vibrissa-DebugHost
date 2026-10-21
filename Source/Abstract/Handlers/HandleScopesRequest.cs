using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DebugServer;

partial class BytecodeDebugAdapterBase
{
    protected override ScopesResponse HandleScopesRequest(ScopesArguments arguments)
    {
        Log.Trace($"[Handler] Scopes");

        if (!Processor.IsValid) return new ScopesResponse();

        using (SyncLock.EnterScope())
        {
            for (int i = 0; i < StackFrames.Count; i++)
            {
                FetchedFrame item = StackFrames[i];
                if (item.Id != arguments.FrameId) continue;
                List<Scope> result = new();
                //if (i == 0)
                //{
                //    result.Add(new Scope()
                //    {
                //        Name = "Registers",
                //        PresentationHint = Scope.PresentationHintValue.Registers,
                //        VariablesReference = int.MaxValue,
                //    });
                //}
                foreach (FetchedScope scope in item.Scopes)
                {
                    (string name, Scope.PresentationHintValue presentationHint) = scope.Kind switch
                    {
                        FetchedScopeKind.ReturnValue => ("ReturnValue", Scope.PresentationHintValue.ReturnValue),
                        FetchedScopeKind.Locals => ("Locals", Scope.PresentationHintValue.Locals),
                        FetchedScopeKind.Arguments => ("Arguments", Scope.PresentationHintValue.Arguments),
                        FetchedScopeKind.Internals => ("Internals", Scope.PresentationHintValue.Locals),
                        FetchedScopeKind.Globals => ("Globals", Scope.PresentationHintValue.Locals),
                        _ => throw new UnreachableException(),
                    };
                    result.Add(new Scope()
                    {
                        NamedVariables = scope.Variables.Length,
                        Name = name,
                        PresentationHint = presentationHint,
                        VariablesReference = scope.Id,
                    });
                }
                return new ScopesResponse(result);
            }
        }

        return new ScopesResponse();
    }
}
