using MatthiWare.CommandLine;

namespace merge_subs;

public class Arguments
{
  public string MkvPath { get; set; }
  public string SubPath { get; set; }
  public string SubPattern { get; set; }
  public string AudioPath { get; set; }
  public string AudioExtension { get; set; }
  public string MkvToolnix { get; set; }

  public static Arguments Parse( string[] args )
  {
    var parser = new CommandLineParser<Arguments>();

    parser.Configure( o => o.SubPath )
      .Name( "s", "sub" )
      .Description( "path to subtitle files" )
      .Required();

    parser.Configure( o => o.MkvPath )
      .Name( "m", "mkv" )
      .Description( "path to mkv files" )
      .Default( "." )
      .Required( false );

    parser.Configure( o => o.AudioPath )
      .Name( "a", "audio" )
      .Description( "path to audio files" )
      .Default( null )
      .Required( false );

    parser.Configure( o => o.AudioExtension )
      .Name( "ae" )
      .Description( "audio files extension" )
      .Default( ".mka" )
      .Required( false );

    parser.Configure( o => o.SubPattern )
      .Name( "se" )
      .Description( "subtitle files pattern" )
      .Default( "*.srt" )
      .Required( false );

    parser.Configure( o => o.MkvToolnix )
      .Name( "t", "mkvToolnix" )
      .Description( "mkv toolnix installation directory" )
      .Default( "C:\\Program Files\\MKVToolNix" )
      .Required( false );

    var parserResult = parser.Parse( args );

    if ( parserResult.HasErrors )
      throw new Exception( string.Join( Environment.NewLine, parserResult.Errors.Select( x => x.Message ) ) );

    var arguments = parserResult.Result;

    Console.WriteLine( $"MkvPath:    {arguments.MkvPath}" );
    Console.WriteLine( $"SubPath:    {arguments.SubPath}" );
    Console.WriteLine( $"SubExt:     {arguments.SubPattern}" );
    Console.WriteLine( $"MkvToolnix: {arguments.MkvToolnix}" );

    if ( !Directory.Exists( arguments.MkvPath ) )
      throw new Exception( $"mkv path points to invalid directory: {arguments.MkvPath}" );

    if ( !Directory.Exists( arguments.SubPath ) )
      throw new Exception( $"sub path points to invalid directory: {arguments.SubPath}" );

    if ( !Directory.Exists( arguments.MkvToolnix ) )
      throw new Exception( $"mkv toolnix points to invalid directory: {arguments.MkvToolnix}" );

    return parserResult.Result;
  }
}