function GetFolderPath([string] $guid)
{
# List of known folder IDs:
# https://msdn.microsoft.com/en-us/library/windows/desktop/dd378457%28v=vs.85%29.aspx 

$t = ([System.Management.Automation.PSTypeName][PInvoke.KnownFolders]).Type

if (-not ($t)) {
   $sig = '
        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath(ref Guid guid, int flags, IntPtr token, out IntPtr path);

        public static string GetKnownFolder(string guid)
        {
            var g = new Guid(guid);
            IntPtr path;
            if (SHGetKnownFolderPath(ref g, 0, IntPtr.Zero, out path) == 0)
            {
                var s = Marshal.PtrToStringUni(path);
                Marshal.FreeCoTaskMem(path);
                return s;
            }

            return null;
        }
     '
    $t = Add-Type -MemberDefinition $sig -Name 'KnownFolders' -Namespace 'PInvoke' -PassThru
    }

$folder = $t::GetKnownFolder($guid)
return $folder
}

function Use-PublisherSettings()
{
[CmdletBinding()] 

# get downloads folder
$downloads = GetFolderPath('374DE290-123F-4565-9164-39C4925E467B');

$settings = Get-ChildItem -Path $downloads -Filter '*.publishsettings' | sort-object -property CreationTimeUtc -Descending
if ($settings.Length -eq 0) {
  Write-Host "No publish settings found, please call 'Get-AzurePublishSettingsFile'"
  return
}

$pub = [IO.Path]::Combine($downloads,$settings[0])
Import-AzurePublishSettingsFile -PublishSettingsFile $pub -ErrorAction Stop
}
