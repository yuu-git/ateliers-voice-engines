namespace Ateliers.Voice.Engins.VoicePeakTools.UnitTests
{
    public class VppParserTests
    {
        [Fact]
        public void ParseVppFile_ShouldParseDefaultTemplateCorrectly()
        {
            var obj = new VppParser();
            var vppFile = obj.ParseVppFile("./SimpleData/DefaultTemplate.vpp");

            // vppFile の基本情報の確認
            Assert.Equal("1.2.19", vppFile.Version);
            Assert.NotNull(vppFile.Project);

            // project.params の確認
            Assert.NotNull(vppFile.Project.Params);
            Assert.Equal(1.0, vppFile.Project.Params.Speed);
            Assert.Equal(0.0, vppFile.Project.Params.Pitch);
            Assert.Equal(1.0, vppFile.Project.Params.Pause);
            Assert.Equal(1.0, vppFile.Project.Params.Volume);

            // project.emotions の確認
            Assert.Equal(9, vppFile.Project.Emotions.Count);
            Assert.Equal(0.0, vppFile.Project.Emotions["happy"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["angry"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["sad"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["ochoushimono"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["hightension"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["buchigire"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["nageki"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["sagesumi"]);
            Assert.Equal(0.0, vppFile.Project.Emotions["sasayaki"]);

            // project.grobalemotions の確認
            Assert.Equal(2, vppFile.Project.GlobalEmotions.Count);
            Assert.Equal("フリモメン", vppFile.Project.GlobalEmotions[0].Narrator);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[0].Emotions["normal"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[0].Emotions["happy"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[0].Emotions["angry"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[0].Emotions["sad"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[0].Emotions["ochoushimono"]);
            Assert.Equal("夏色花梨", vppFile.Project.GlobalSettings[1].Narrator);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[1].Emotions["neutral"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[1].Emotions["hightension"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[1].Emotions["buchigire"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[1].Emotions["sagesumi"]);
            Assert.Equal(0.0, vppFile.Project.GlobalEmotions[1].Emotions["sasayaki"]);

            // project.grobalsettings の確認
            Assert.Equal(2, vppFile.Project.GlobalSettings.Count);
            Assert.Equal("フリモメン", vppFile.Project.GlobalSettings[0].Narrator);
            Assert.NotNull(vppFile.Project.GlobalSettings[0].Params);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[0].Params.Speed);
            Assert.Equal(0.0, vppFile.Project.GlobalSettings[0].Params.Pitch);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[0].Params.Pause);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[0].Params.Volume);
            Assert.Equal("夏色花梨", vppFile.Project.GlobalSettings[1].Narrator);
            Assert.NotNull(vppFile.Project.GlobalSettings[1].Params);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[1].Params.Speed);
            Assert.Equal(0.0, vppFile.Project.GlobalSettings[1].Params.Pitch);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[1].Params.Pause);
            Assert.Equal(1.0, vppFile.Project.GlobalSettings[1].Params.Volume);

            // project.export の確認
            Assert.NotNull(vppFile.Project.Export);
            Assert.Equal(2, vppFile.Project.Export.Mode);
            Assert.Equal(0, vppFile.Project.Export.AudioFormat);
            Assert.Equal(48000, vppFile.Project.Export.SampleRate);
            Assert.False(vppFile.Project.Export.WriteScripts);
            Assert.True(vppFile.Project.Export.WriteText);
            Assert.False(vppFile.Project.Export.WriteSrt);
            Assert.False(vppFile.Project.Export.WriteLab);
            Assert.False(vppFile.Project.Export.NameRule);
            Assert.Equal(0, vppFile.Project.Export.CharCode);
            Assert.Equal(3, vppFile.Project.Export.NameFormats.Count);
            Assert.Equal(1, vppFile.Project.Export.NameFormats[0]);
            Assert.Equal(0, vppFile.Project.Export.NameFormats[1]);
            Assert.Equal(0, vppFile.Project.Export.NameFormats[2]);

            // project.blocks の確認
            Assert.Single(vppFile.Project.Blocks);
            Assert.Equal(2, vppFile.Project.Blocks.First().TimeOffsetMode);
            Assert.Equal(0, vppFile.Project.Blocks.First().TimeOffset);

            // project.blocks.narrator の確認
            Assert.NotNull(vppFile.Project.Blocks.First().Narrator);
            Assert.Equal("夏色花梨", vppFile.Project.Blocks.First().Narrator.Key);
            Assert.Equal("japanese", vppFile.Project.Blocks.First().Narrator.Language);
            Assert.Equal(-1, vppFile.Project.Blocks.First().Narrator.NarratorVersion);

            // project.blocks.params の確認
            Assert.NotNull(vppFile.Project.Blocks.First().Params);
            Assert.Equal(1.0, vppFile.Project.Blocks.First().Params.Speed);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Params.Pitch);
            Assert.Equal(1.0, vppFile.Project.Blocks.First().Params.Pause);
            Assert.Equal(1.0, vppFile.Project.Blocks.First().Params.Volume);

            // project.blocks.emotions の確認
            Assert.Equal(9, vppFile.Project.Blocks.First().Emotions.Count);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["happy"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["angry"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["sad"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["ochoushimono"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["hightension"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["buchigire"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["nageki"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["sagesumi"]);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().Emotions["sasayaki"]);

            // project.blocks.sentence-list の確認
            Assert.Single(vppFile.Project.Blocks.First().SentenceList);
            Assert.Equal("プロジェクトの参考用のテンプレート音声です。", vppFile.Project.Blocks.First().SentenceList.First().Text);
            Assert.True(vppFile.Project.Blocks.First().SentenceList.First().HasEos);

            // project.blocks.sentence-list.tokens の確認
            Assert.Equal(9, vppFile.Project.Blocks.First().SentenceList.First().Tokens.Count);

            Assert.Equal("プロジェクト", vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].S);
            Assert.Equal(4096, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Pos);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Lang);
            Assert.False(vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Pe);
            Assert.Equal(5, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl.Count);
            Assert.Equal("プ", vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].S);
            Assert.True(vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].Ig);
            Assert.Equal(8192, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].A);
            Assert.Equal(0.0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].I);
            Assert.False(vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].U);
            Assert.Equal(2, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P.Count);
            Assert.Equal("p", vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[0].S);
            Assert.Equal(1.0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[0].D);
            Assert.False(vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[0].N);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[0].Dt);
            Assert.Equal("u", vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[1].S);
            Assert.Equal(1.0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[1].D);
            Assert.True(vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[1].N);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].Syl[0].P[1].Dt);

            Assert.Equal(2, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R8.Count);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R8[0]);
            Assert.Equal(18, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R8[1]);
            Assert.Equal(2, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R32.Count);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R32[0]);
            Assert.Equal(6, vppFile.Project.Blocks.First().SentenceList.First().Tokens[0].R32[1]);

            Assert.Equal("の", vppFile.Project.Blocks.First().SentenceList.First().Tokens[1].S);
            Assert.Equal("参考", vppFile.Project.Blocks.First().SentenceList.First().Tokens[2].S);
            Assert.Equal("用", vppFile.Project.Blocks.First().SentenceList.First().Tokens[3].S);
            Assert.Equal("の", vppFile.Project.Blocks.First().SentenceList.First().Tokens[4].S);
            Assert.Equal("テンプレート", vppFile.Project.Blocks.First().SentenceList.First().Tokens[5].S);
            Assert.Equal("音声", vppFile.Project.Blocks.First().SentenceList.First().Tokens[6].S);
            Assert.Equal("です", vppFile.Project.Blocks.First().SentenceList.First().Tokens[7].S);
            Assert.Equal("。", vppFile.Project.Blocks.First().SentenceList.First().Tokens[8].S);

            /*
            Token: �v���W�F�N�g, Pos: 4096, Lang: 0, Pe: False, Syl count: 5
            Token: ��, Pos: 4104, Lang: 0, Pe: False, Syl count: 1
            Token: �Q�l, Pos: 4096, Lang: 0, Pe: False, Syl count: 4
            Token: �p, Pos: 4106, Lang: 0, Pe: False, Syl count: 2
            Token: ��, Pos: 4104, Lang: 0, Pe: False, Syl count: 1
            Token: �e���v���[�g, Pos: 4096, Lang: 0, Pe: False, Syl count: 6
            Token: ����, Pos: 4096, Lang: 0, Pe: False, Syl count: 4
            Token: �ł�, Pos: 4103, Lang: 0, Pe: False, Syl count: 2
            Token: �B, Pos: 4107, Lang: 0, Pe: False, Syl count: 1
             
            foreach (var token in vppFile.Project.Blocks.First().SentenceList.First().Tokens)
            {
                Console.WriteLine($"Token: {token.S}, Pos: {token.Pos}, Lang: {token.Lang}, Pe: {token.Pe}, Syl count: {token.Syl.Count}");
            }
            */

            // project.blocks.sentence-ranges の確認
            Assert.Single(vppFile.Project.Blocks.First().SentenceRanges);
            Assert.Equal(2, vppFile.Project.Blocks.First().SentenceRanges.First().Count);
            Assert.Equal(0, vppFile.Project.Blocks.First().SentenceRanges.First()[0]);
            Assert.Equal(22, vppFile.Project.Blocks.First().SentenceRanges.First()[1]);

            // project.voices の確認
            Assert.Equal(2, vppFile.Voices.Count);
            Assert.Contains("フリモメン", vppFile.Voices.Keys);
            Assert.Equal(100, vppFile.Voices["フリモメン"].Latest);
            Assert.Equal("フリモメン", vppFile.Voices["フリモメン"].Nid);
            Assert.Contains("夏色花梨", vppFile.Voices.Keys);
            Assert.Equal(100, vppFile.Voices["夏色花梨"].Latest);
            Assert.Equal("夏色花梨", vppFile.Voices["夏色花梨"].Nid);

        }
    }
}