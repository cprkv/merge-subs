
using merge_subs;
using System.Diagnostics;

var arguments = Arguments.Parse( args );
var mkvFiles = Directory.GetFiles( arguments.MkvPath, "*.mkv" );
var subFiles = Directory.GetFiles( arguments.SubPath, arguments.SubPattern );

if ( mkvFiles.Length != subFiles.Length )
  Console.Error.WriteLine( $"mkv files count ({mkvFiles.Length}) not match sub files count ({subFiles.Length})" );

var mergedDir = Path.Join( arguments.MkvPath, "merged" );
if ( !Directory.Exists( mergedDir ) )
  Directory.CreateDirectory( mergedDir );

foreach ( var subPath in subFiles )
{
  var name = Path.GetFileNameWithoutExtension( Path.GetFileName( subPath ) );
  var mkvPath = Path.Join( arguments.MkvPath, Path.ChangeExtension( name, ".mkv" ) );

  if ( !File.Exists( mkvPath ) )
  {
    Console.Error.WriteLine( $"corresponding mkv file for '{subPath}' not found. searching was: '{mkvPath}'" );
    continue;
  }

  var audioFile = arguments.AudioPath != null
    ? Path.Join( arguments.AudioPath, name + arguments.AudioExtension )
    : null;

  if ( audioFile != null && !File.Exists( audioFile ) )
  {
    Console.Error.WriteLine( $"corresponding audio file for '{subPath}' not found. searching was: '{audioFile}'" );
    audioFile = null;
  }

  var mergedPath = Path.Join( mergedDir, Path.ChangeExtension( name, ".mkv" ) );

  if ( File.Exists( mergedPath ) )
  {
    Console.WriteLine( $"WARNING: {mergedPath} already exists. skipping" );
    continue;
  }

  {
    var commonPrefix = CommonPrefix( mkvPath, subPath, mergedPath );
    Console.WriteLine();
    Console.WriteLine(
      $"'{mkvPath.Remove( 0, commonPrefix.Length )}' \n" +
      $"'{subPath.Remove( 0, commonPrefix.Length )}' \n" +
      $"    => '{mergedPath.Remove( 0, commonPrefix.Length )}'" );
  }

  RunMkvMerge( arguments, "-o", mergedPath, mkvPath, subPath, audioFile );
}

Process.Start( "explorer.exe", NormalizePath( mergedDir ) );
Console.WriteLine( "DONE!" );


static string CommonPrefix( params string[] strings )
{
  var commonPrefix = strings.FirstOrDefault() ?? "";

  foreach ( var s in strings )
  {
    var potentialMatchLength = Math.Min( s.Length, commonPrefix.Length );

    if ( potentialMatchLength < commonPrefix.Length )
      commonPrefix = commonPrefix.Substring( 0, potentialMatchLength );

    for ( var i = 0; i < potentialMatchLength; i++ )
    {
      if ( s[ i ] != commonPrefix[ i ] )
      {
        commonPrefix = commonPrefix.Substring( 0, i );
        break;
      }
    }
  }

  return commonPrefix;
}

static string NormalizePath( string path )
{
  return Path.GetFullPath( new Uri( path ).LocalPath )
    .TrimEnd( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
}

static void RunMkvMerge( Arguments arguments, params string[] args )
{
  using var process = new Process
  {
    StartInfo = new ProcessStartInfo
    {
      FileName = Path.Join( arguments.MkvToolnix, "mkvmerge.exe" ),
      UseShellExecute = false,
      RedirectStandardOutput = true,
      CreateNoWindow = true,
    }
  };

  foreach ( var arg in args )
  {
    if ( arg != null )
    {
      process.StartInfo.ArgumentList.Add( arg );
    }
  }

  process.Start();
  process.WaitForExit();

  if ( process.ExitCode != 0 )
  {
    Console.Error.WriteLine( $"process exited with bad exit code: {process.ExitCode}" );
  }
}