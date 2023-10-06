using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Input;
using MonoMod;
using ReLogic.Localization.IME;

namespace Tomat.TerrariaModernizer.ReLogic.Patches;

[MonoModPatch("global::ReLogic.Localization.IME.FnaIme")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal abstract class FnaImeHookTextPatch : FnaIme {
    public extern void orig_ctor();

    [MonoModConstructor]
    public void ctor() {
        TextInputEXT.TextInput += OnCharCallback;

        orig_ctor();
    }

    protected extern void orig_Dispose(bool disposing);

    protected new void Dispose(bool disposing) {
        orig_Dispose(disposing);

        if (disposing)
            TextInputEXT.TextInput -= OnCharCallback;
    }

    [MonoModIgnore]
    private extern void OnCharCallback(char key);
}
