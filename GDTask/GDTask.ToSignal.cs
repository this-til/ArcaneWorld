using Godot;
using System.Threading;

namespace Fractural.Tasks
{
    public partial struct GDTask
    {
        public static async GDTask<Variant[]> ToSignal(GodotObject self, StringName signal)
        {
            return await self.ToSignal(self, signal);
        }

        public static async GDTask<Variant[]> ToSignal(GodotObject self, StringName signal, CancellationToken ct)
        {
            GDTaskCompletionSource<Variant[]> tcs = new GDTaskCompletionSource<Variant[]>();
            ct.Register(() => tcs.TrySetCanceled(ct));
            Create(async () =>
            {
                Variant[] result = await self.ToSignal(self, signal);
                tcs.TrySetResult(result);
            }).Forget();
            return await tcs.Task;
        }
    }
}
