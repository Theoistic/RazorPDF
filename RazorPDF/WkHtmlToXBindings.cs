using System.Runtime.InteropServices;
using System.Text;

namespace RazorPDF;

internal enum CustomUnmanagedType
{
    LPUTF8Str = 48
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void VoidCallback(IntPtr converter);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void IntCallback(IntPtr converter, int integer);

internal class PdfTools : IDisposable
{
    public bool IsLoaded { get; private set; }

    public PdfTools()
    {
        IsLoaded = false;
    }

    public void Load()
    {

        if (IsLoaded)
        {
            return;
        }

        if (WkHtmlToXBindings.wkhtmltopdf_init(0) == 1)
        {
            IsLoaded = true;
        }
    }

    public bool ExtendedQt()
    {
        return WkHtmlToXBindings.wkhtmltopdf_extended_qt() == 1 ? true : false;
    }

    public string GetLibraryVersion()
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_version());
    }

    public IntPtr CreateGlobalSettings()
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_global_settings();
    }

    public int SetGlobalSetting(IntPtr settings, string name, string value)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_global_setting(settings, name, value);
    }

    public unsafe string GetGlobalSetting(IntPtr settings, string name)
    {
        //default const char * size is 2048 bytes 
        byte[] buffer = new byte[2048];

        fixed (byte* tempBuffer = buffer)
        {
            WkHtmlToXBindings.wkhtmltopdf_get_global_setting(settings, name, tempBuffer, buffer.Length);
        }

        return GetString(buffer);
    }

    public void DestroyGlobalSetting(IntPtr settings)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_global_settings(settings);
    }

    public IntPtr CreateObjectSettings()
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_object_settings();
    }

    public int SetObjectSetting(IntPtr settings, string name, string value)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_object_setting(settings, name, value);
    }

    public unsafe string GetObjectSetting(IntPtr settings, string name)
    {
        //default const char * size is 2048 bytes 
        byte[] buffer = new byte[2048];

        fixed (byte* tempBuffer = buffer)
        {
            WkHtmlToXBindings.wkhtmltopdf_get_object_setting(settings, name, tempBuffer, buffer.Length);
        }

        return GetString(buffer);
    }

    public void DestroyObjectSetting(IntPtr settings)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_object_settings(settings);
    }

    public IntPtr CreateConverter(IntPtr globalSettings)
    {
        return WkHtmlToXBindings.wkhtmltopdf_create_converter(globalSettings);
    }

    public void AddObject(IntPtr converter, IntPtr objectSettings, byte[] data)
    {
        WkHtmlToXBindings.wkhtmltopdf_add_object(converter, objectSettings, data);
    }

    public void AddObject(IntPtr converter, IntPtr objectSettings, string data)
    {
        WkHtmlToXBindings.wkhtmltopdf_add_object(converter, objectSettings, data);
    }

    public bool DoConversion(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_convert(converter);
    }

    public void DestroyConverter(IntPtr converter)
    {
        WkHtmlToXBindings.wkhtmltopdf_destroy_converter(converter);
    }

    public byte[] GetConversionResult(IntPtr converter)
    {
        IntPtr resultPointer;

        int length = WkHtmlToXBindings.wkhtmltopdf_get_output(converter, out resultPointer);
        var result = new byte[length];
        Marshal.Copy(resultPointer, result, 0, length);

        return result;
    }

    public int SetPhaseChangedCallback(IntPtr converter, VoidCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_phase_changed_callback(converter, callback);
    }

    public int SetProgressChangedCallback(IntPtr converter, VoidCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_progress_changed_callback(converter, callback);
    }

    public int SetFinishedCallback(IntPtr converter, IntCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_finished_callback(converter, callback);
    }

    public int SetWarningCallback(IntPtr converter, StringCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_warning_callback(converter, callback);
    }

    public int SetErrorCallback(IntPtr converter, StringCallback callback)
    {
        return WkHtmlToXBindings.wkhtmltopdf_set_error_callback(converter, callback);
    }

    public int GetPhaseCount(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_phase_count(converter);
    }

    public int GetCurrentPhase(IntPtr converter)
    {
        return WkHtmlToXBindings.wkhtmltopdf_current_phase(converter);
    }

    public string GetPhaseDescription(IntPtr converter, int phase)
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_phase_description(converter, phase));
    }

    public string GetProgressString(IntPtr converter)
    {
        return Marshal.PtrToStringAnsi(WkHtmlToXBindings.wkhtmltopdf_progress_string(converter));
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            //free unmanaged resources (unmanaged objects) and override a finalizer below.
            WkHtmlToXBindings.wkhtmltopdf_deinit();
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~PdfTools()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion

    private string GetString(byte[] buffer)
    {
        string value = "";

        int walk = 0;

        while (walk < buffer.Length && buffer[walk] != 0)
        {
            walk++;
        }

        value = Encoding.UTF8.GetString(buffer, 0, walk);

        return value;
    }
}

internal unsafe static class WkHtmlToXBindings
{
    const string DLLNAME = "libwkhtmltox";

    const CharSet CHARSET = CharSet.Unicode;

    #region HTML to PDF bindings
    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_extended_qt();
    
    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_version();

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_init(int useGraphics);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_deinit();

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_create_global_settings();

    [DllImport(DLLNAME, CharSet = CHARSET)]
    public static extern int wkhtmltopdf_set_global_setting(IntPtr settings,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string name,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string value);


    [DllImport(DLLNAME, CharSet = CHARSET)]
    public static unsafe extern int wkhtmltopdf_get_global_setting(IntPtr settings,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string name,
        byte* value, int valueSize);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_destroy_global_settings(IntPtr settings);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_create_object_settings();

    [DllImport(DLLNAME, CharSet = CHARSET)]
    public static extern int wkhtmltopdf_set_object_setting(IntPtr settings,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string name,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string value);

    [DllImport(DLLNAME, CharSet = CHARSET)]
    public static unsafe extern int wkhtmltopdf_get_object_setting(IntPtr settings,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
        string name,
        byte* value, int valueSize);
    
    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_destroy_object_settings(IntPtr settings);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern void wkhtmltopdf_add_object(IntPtr converter, 
        IntPtr objectSettings, 
        byte[] data);
    
    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern void wkhtmltopdf_add_object(IntPtr converter,
        IntPtr objectSettings,
        [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string data);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool wkhtmltopdf_convert(IntPtr converter);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_phase_count(IntPtr converter);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_current_phase(IntPtr converter);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);

    [DllImport(DLLNAME, CharSet = CHARSET, CallingConvention = CallingConvention.Cdecl)]
    public static extern int wkhtmltopdf_http_error_code(IntPtr converter);

    #endregion

    #region Image to  PDF bindings


    #endregion
}
