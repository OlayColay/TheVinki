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
        }

        private static void RemoveMusicHooks()
        {
            On.Music.MusicPiece.StartPlaying -= MusicPiece_StartPlaying;
            On.Music.MusicPiece.Update -= MusicPiece_Update;
        }

        private static void MusicPiece_StartPlaying(On.Music.MusicPiece.orig_StartPlaying orig, MusicPiece self)
        {
            orig(self);

            if (self.subTracks.Count < 1 || self.IsProcedural || self.subTracks[0].source.clip == null)
            {
                return;
            }

            AudioClip loadedClip;
            string checkPath3 = string.Concat(
                [
                    "Music",
                    Path.DirectorySeparatorChar.ToString(),
                    "Songs",
                    Path.DirectorySeparatorChar.ToString(),
                    self.subTracks[0].trackName,
                    ".ogg"
                ]);
            string modPathCheck2 = AssetManager.ResolveFilePath(checkPath3);
            bool flag9 = !Application.isConsolePlatform && modPathCheck2 != Path.Combine(RWCustom.Custom.RootFolderDirectory(), checkPath3.ToLowerInvariant()) && File.Exists(modPathCheck2);
            if (flag9)
            {
                loadedClip = AssetManager.SafeWWWAudioClip("file://" + modPathCheck2, false, false, AudioType.OGGVORBIS); 
                Plugin.curUpdatesPerBeat = Mathf.RoundToInt(2400f / UniBpmAnalyzer.AnalyzeBpm(loadedClip));
            }
            else
            {
                // It's a vanilla song
                Plugin.curUpdatesPerBeat = 29;
            }
            Plugin.curUpdatesSinceSong = 0;
        }

        private static void MusicPiece_Update(On.Music.MusicPiece.orig_Update orig, MusicPiece self)
        {
            orig(self);

            if (!self.startedPlaying)
            {
                return;
            }

            Plugin.curUpdatesSinceSong++;
        }
    }
}