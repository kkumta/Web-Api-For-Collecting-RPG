using System;

namespace WebApiForCollectingRPG.Util
{
    public class Game
    {
        // 누적 확률을 이용해 하나의 index를 반환하는 메서드
        public static Int32 Choose(double[] probs)
        {

            double total = 0;

            foreach (double elem in probs)
            {
                total += elem;
            }

            // 0 ~ 100 중 하나의 정수를 뽑는다.
            double randomPoint = new Random().Next(0, 101);

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }

            return probs.Length - 1;
        }
    }
}
