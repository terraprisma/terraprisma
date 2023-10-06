using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NATUPNPLib;

[ComImport]
[CompilerGenerated]
[Guid("6F10711F-729B-41E5-93B8-F21D0F818DF1")]
[TypeIdentifier]
public interface IStaticPortMapping
{
	[DispId(3)]
	int InternalPort
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[DispId(3)]
		get;
	}

	[DispId(4)]
	string Protocol
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[DispId(4)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(5)]
	string InternalClient
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[DispId(5)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	void _VtblGap1_2();
}
