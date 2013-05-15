/*
Copyright 2011, Andrew Polar

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HACtest
{
    class StopWordFilter
    {
        private char[] m_stopWords = null;
        private int m_nSize = 0;
        private int[] m_oneByteLookup = new int[256];
        private int[] m_twoByteLookup = new int[0xffff + 1];
        private int[] m_twoByteSelf = null;

        private void makeOneByteSelfAddress()
        {
            for (int i = 0; i < 256; ++i)
            {
                m_oneByteLookup[i] = 0x00;
            }
            int scan = 0x202020;
            for (int i = 0; i < m_nSize; ++i)
            {
                scan <<= 8;
                scan |= m_stopWords[i];
                scan &= 0xffffff;
                if ((scan >> 16) == 0x20 && (scan & 0xff) == 0x20)
                {
                    m_oneByteLookup[m_stopWords[i - 1]] = i - 1;
                }
            }
        }

        void makeTwoByteSelfAddress()
        {
            m_twoByteSelf = new int[m_nSize + 1];
            for (int i = 0; i < 0xffff + 1; ++i)
            {
                m_twoByteLookup[i] = 0x00;
            }
            for (int i = 0; i < m_nSize + 1; ++i)
            {
                m_twoByteSelf[i] = 0x00;
            }
            int scan = 0x202020;
            for (int i = 0; i < m_nSize; ++i)
            {
                scan <<= 8;
                scan |= m_stopWords[i];
                scan &= 0xffffff;
                if ((scan >> 16) == 0x20 && (((scan >> 8) & 0xff) != 0x20 && (scan & 0xff) != 0x20))
                {
                    m_twoByteSelf[i+1] = m_twoByteLookup[scan & 0xffff];
                    m_twoByteLookup[scan & 0xffff] = i+1;
                }
            }
        }

        public bool isThere(byte[] word, int len)
        {
            if (word == null) return false;
            if (len <= 0) len = word.Length;

            if (len == 0) return false;
            if (len == 1)
            {
                if (m_oneByteLookup[word[0]] > 0) return true;
                else return false;
            }
            if (len == 2)
            {
                int scan = word[0];
                scan <<= 8;
                scan |= word[1];
                if (m_twoByteLookup[scan] > 0) return true;
                else return false;
            }
            if (len > 2)
            {
                int scan = word[0];
                scan <<= 8;
                scan |= word[1];
                int pos = m_twoByteLookup[scan];
                if (pos == 0) return false;
                while (true)
                {
                    bool isOK = true;
                    int n = pos;
                    for (int k = 2; k < len; ++k)
                    {
                        if (m_stopWords[n] != word[k])
                        {
                            isOK = false;
                            break;
                        }
                        ++n;
                        if (n == m_nSize) return false;
                    }
                    if (isOK == true) return true;
                    pos = m_twoByteSelf[pos];
                    if (pos == 0) return false;
                }
            }
            return false;
        }

        public StopWordFilter()
        {
            string s = "";
            s += "about a above across after again against all almost alone along ";
            s += "already also although always among an and another any anybody ";
            s += "0 1 2 3 4 5 6 7 8 9 ";
            s += "anyone anything anywhere are area areas around as ask asked asking ";
            s += "asks at away b back backed backing backs be became because become ";
            s += "becomes been before began behind being beings best better between ";
            s += "big both but by c came can cannot case cases certain certainly ";
            s += "clear clearly come could d did differ different differently do ";
            s += "does done down downed downing downs during e each early either ";
            s += "end ended ending ends enough even evenly ever every everybody ";
            s += "everyone everything everywhere f face faces fact facts far felt ";
            s += "few find finds first for four from full fully further furthered ";
            s += "furthering furthers g gave general generally get gets give given ";
            s += "gives go going good goods got great greater greatest group grouped ";
            s += "grouping groups h had has have having he her here herself high ";
            s += "higher highest him himself his how however i if important in ";
            s += "interest interested interesting interests into is it its itself ";
            s += "j just k keep keeps kind knew know known knows l large largely ";
            s += "last later latest least less let lets like likely long longer longest ";
            s += "m made make making man many may me member members men might more ";
            s += "most mostly mr mrs much must my myself n necessary need needed ";
            s += "needing needs never new new newer newest next no nobody non noone ";
            s += "not nothing now nowhere number numbers o of off often old older ";
            s += "oldest on once one only open opened opening opens or order ordered ";
            s += "ordering orders other others our out over p part parted parting ";
            s += "parts per perhaps place places point pointed pointing points possible ";
            s += "present presented presenting presents problem problems put puts q ";
            s += "quite r rather really right right room rooms s said same saw say ";
            s += "says second seconds see seem seemed seeming seems sees several shall ";
            s += "she should show showed showing shows side sides since small smaller ";
            s += "smallest so some somebody someone something somewhere state states ";
            s += "still such sure t take taken than that the their them then there ";
            s += "therefore these they thing things think thinks this those though ";
            s += "thought thoughts three through thus to today together too took toward ";
            s += "turn turned turning turns two u under until up upon us use used uses ";
            s += "v very w want wanted wanting wants was way ways we well wells went ";
            s += "were what when where whether which while who whole whose why will ";
            s += "with within without work worked working works would x y year years ";
            s += "yet you young younger youngest your z yours";
            s += "a aby ach acz aczkolwiek aj albo ale alez ani az";
            s += "bardziej bardzo beda bedzie bez bo bowiem by byc byl byla byli bylo byly bynajmniej";
            s += "cala cali caly ci cie ciebie co cokolwiek cos czasami czasem czemu czy czyli";
            s += "daleko dla dlaczego dlatego do dobrze dokad dosc duzo dwa dwaj dwie dwoje dzis dzisiaj";
            s += "gdy gdyby gdyz gdzie gdziekolwiek gdzies go";
            s += "i ich ile im inna inne inny innych iz";
            s += "ja jak jakas jakby jaki jakichs jakie jakis jakiz jakkolwiek jako jakos je jeden jedna jednak jednakze jedno jego jej jemu jesli jest jestem jeszcze jezeli juz";
            s += "kazdy kiedy kierunku kilka kims kto ktokolwiek ktora ktore ktorego ktorej ktory ktorych ktorym ktorzy ktos ku";
            s += "lat lecz lub";
            s += "ma maja malo mam mi miedzy mimo mna mnie moga moi moim moj moja moje moze mozliwe mozna mu musi my";
            s += "na nad nam nami nas nasi nasz nasza nasze naszego naszych natomiast natychmiast nawet nia nic nich nie niech niego niej niemu nigdy nim nimi niz no";
            s += "o obok od okolo on ona one oni ono oraz oto owszem";
            s += "pan pana pani po pod podczas pomimo ponad poniewaz powinien powinna powinni powinno poza prawie przeciez przed przede przedtem przez przy";
            s += "roku rowniez";
            s += "sa sam sama sie skad soba sobie sposob swoje";
            s += "ta tak taka taki takie takze tam te tego tej temu ten teraz tez to toba tobie totez totoba trzeba tu tutaj twoi twoim twoj twoja twoje twym ty tych tylko tym";
            s += "u w wam wami was wasi wasz wasza wasze we wedlug wiec wiecej wiele wielu wlasnie wszyscy wszystkich wszystkie wszystkim wszystko wtedy wy";
            s += "z za zaden zadna zadne zadnych zapewne zawsze ze zeby zeznowu zl znow znowu zostal";
            m_stopWords = s.ToCharArray();

            m_nSize = m_stopWords.Length;
            makeOneByteSelfAddress();
            makeTwoByteSelfAddress();
        }
    }
}
