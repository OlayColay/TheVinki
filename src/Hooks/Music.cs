using AssetBundles;
using Music;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyMusicHooks()
        {
            On.Music.MusicPiece.StartPlaying += MusicPiece_StartPlaying;

            On.Music.Song.FadeOut += Song_FadeOut;
        }

        private static void RemoveMusicHooks()
        {
            On.Music.MusicPiece.StartPlaying -= MusicPiece_StartPlaying;

            On.Music.Song.FadeOut -= Song_FadeOut;
        }

        private static async void MusicPiece_StartPlaying(On.Music.MusicPiece.orig_StartPlaying orig, MusicPiece self)
        {
            orig(self);

            if (self.subTracks.Count < 1 || self.IsProcedural || self.subTracks[0].source.clip == null)
            {
                return;
            }

            string trackName = self.subTracks[0].trackName;
            if (Plugin.manualSongMsPerBeat.ContainsKey(trackName))
            {
                // It's a vanilla song, or it was added to the SongTempos list
                Plugin.curMsPerBeat = Plugin.manualSongMsPerBeat[trackName];
                Plugin.curAudioSource = self.subTracks[0].source;
                Plugin.curPlayingSong = trackName;
                return;
            }

            string checkPath3 = string.Concat(
                [
                    "Music",
                    Path.DirectorySeparatorChar.ToString(),
                    "Songs",
                    Path.DirectorySeparatorChar.ToString(),
                    trackName,
                    ".ogg"
                ]);
            string modPathCheck2 = AssetManager.ResolveFilePath(checkPath3);
            if (!Application.isConsolePlatform && modPathCheck2 != Path.Combine(RWCustom.Custom.RootFolderDirectory(), checkPath3.ToLowerInvariant()) && File.Exists(modPathCheck2))
            {
                _ = AsyncLoadClip(modPathCheck2);
                Plugin.curAudioSource = self.subTracks[0].source;
                Plugin.curPlayingSong = trackName;
            }
        }

        private static async Task AsyncLoadClip(string path)
        {
            AudioClip loadedClip = await Task.Run(() => AssetManager.SafeWWWAudioClip("file://" + path, false, false, AudioType.OGGVORBIS));
            Plugin.curMsPerBeat = await Task.Run(() => 60000f / UniBpmAnalyzer.AnalyzeBpm(loadedClip));
        }

        private static void Song_FadeOut(On.Music.Song.orig_FadeOut orig, Song self, float speed)
        {
            orig(self, speed);

            if (self.subTracks[0]?.trackName == Plugin.curPlayingSong)
            {
                Plugin.curMsPerBeat = 0;
            }
        }
    }
}