﻿using static DataPearl.AbstractDataPearl;
using static Menu.MenuScene;
using static Menu.SlideShow;

namespace Vinki;

public static class Enums
{
    public static SlugcatStats.Name TheVinki = new(nameof(TheVinki), false);
    
    public static class SSOracle
    {
        public static Conversation.ID Vinki_SSConvoFirstMeet = new(nameof(Vinki_SSConvoFirstMeet), true);
        public static Conversation.ID Vinki_SSConvoFirstLeave = new(nameof(Vinki_SSConvoFirstLeave), true);
        
        public static SSOracleBehavior.Action Vinki_SSActionGeneral = new(nameof(Vinki_SSActionGeneral), true);
        public static SSOracleBehavior.Action Vinki_SSActionGetOut = new(nameof(Vinki_SSActionGetOut), true);
		public static SSOracleBehavior.SubBehavior.SubBehavID Vinki_SSSubBehavGeneral = new(nameof(Vinki_SSSubBehavGeneral), true);

        public static SlugcatStats.Name VinkiPebbles = new(nameof(VinkiPebbles), true);
    }
}