using AssetBundles;
using Music;
using System;
using System.IO;
using UnityEngine;

namespace Vinki
{
    public static partial class Hooks
    {
        // Add hooks
        private static void ApplyMusicHooks()
        {
            On.Music.MusicPiece.StartPlaying += MusicPiece_StartPlaying;
            On.Music.MusicPiece.Update += MusicPiece_Update;

            On.Music.Song.FadeOut += Song_FadeOut;
        }

        private static void RemoveMusicHooks()
        {
            On.Music.MusicPiece.StartPlaying -= MusicPiece_StartPlaying;
            On.Music.MusicPiece.Update -= MusicPiece_Update;

            On.Music.Song.FadeOut -= Song_FadeOut;
        }

        private static void MusicPiece_StartPlaying(On.Music.MusicPiece.orig_StartPlaying orig, MusicPiece self)
        {
            orig(self);

            if (self.subTracks.Count < 1 || self.IsProcedural || self.subTracks[0].source.clip == null)
            {
                return;
            }

            string trackName = self.subTracks[0].trackName;
            if (Plugin.manualSongUpdatesPerBeat.ContainsKey(trackName))
            {
                // It's a vanilla song
                Plugin.curUpdatesPerBeat = Plugin.manualSongUpdatesPerBeat[trackName];
                Plugin.curUpdatesSinceSong = 0;
                Plugin.curPlayingSong = trackName;
                return;
            }

            AudioClip loadedClip;
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
                loadedClip = AssetManager.SafeWWWAudioClip("file://" + modPathCheck2, false, false, AudioType.OGGVORBIS); 
                Plugin.curUpdatesPerBeat = Mathf.RoundToInt(2400f / UniBpmAnalyzer.AnalyzeBpm(loadedClip));
                Plugin.curUpdatesSinceSong = 0;
                Plugin.curPlayingSong = trackName;
            }
        }

        private static void MusicPiece_Update(On.Music.MusicPiece.orig_Update orig, MusicPiece self)
        {
            orig(self);

            if (self.startedPlaying && self.subTracks[0]?.trackName == Plugin.curPlayingSong)
            {
                Plugin.curUpdatesSinceSong++;
            }
        }

        private static void Song_FadeOut(On.Music.Song.orig_FadeOut orig, Song self, float speed)
        {
            orig(self, speed);

            if (self.subTracks[0]?.trackName == Plugin.curPlayingSong)
            {
                Plugin.curUpdatesPerBeat = 0;
            }
        }
    }
}