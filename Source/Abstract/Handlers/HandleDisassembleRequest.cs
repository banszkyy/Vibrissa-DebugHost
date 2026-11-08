using System.Collections.Generic;
using LanguageCore.Runtime;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DebugServer;

partial class BytecodeDebugAdapterBase
{
    protected override DisassembleResponse HandleDisassembleRequest(DisassembleArguments arguments)
    {
        Log.Trace($"[Handler] Disassemble (offset: {arguments.InstructionOffset}, count: {arguments.InstructionCount}, resolveSymbols: {arguments.ResolveSymbols}, offset: {arguments.Offset}, ref: {arguments.MemoryReference}");

        if (!Processor.IsValid || !int.TryParse(arguments.MemoryReference, out int offset))
        {
            return new DisassembleResponse();
        }
        else
        {
            int start = offset + (arguments.Offset ?? 0);
            int length = arguments.InstructionCount;

            List<DisassembledInstruction> result = new();

            for (int i = 0; i < length; i++)
            {
                int j = i + start;
                if (j < 0) continue;
                if (j >= Processor.Code.Length) break;
                Instruction c = Processor.Code[j];

                DisassembledInstruction r = new()
                {
                    Address = j.ToString(),
                    Instruction = c.ToString(),
                    PresentationHint = DisassembledInstruction.PresentationHintValue.Normal,
                };

                if (DebugInformation.TryGetSourceLocation(j, out SourceCodeLocation sourceLocation, true))
                {
                    r.Line = LineToClient(sourceLocation.Location.Position.Range.Start.Line);
                    r.EndLine = LineToClient(sourceLocation.Location.Position.Range.End.Line);
                    r.Column = LineToClient(sourceLocation.Location.Position.Range.Start.Character);
                    r.EndColumn = LineToClient(sourceLocation.Location.Position.Range.End.Character);
                    r.Location = ToSource(sourceLocation.Location.File);
                }

                if (DebugInformation.TryGetFunctionInformation(j, out FunctionInformation function))
                {
                    r.Symbol = function.ReadableIdentifier();
                }

                result.Add(r);
            }

            return new DisassembleResponse(result);
        }
    }
}
