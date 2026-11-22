using System;

class Program
{
    static void Main()
    {
        var hero = new Character("Артур", maxHp: 100, armor: 5);
        hero.PrintStatus();
        Console.WriteLine();


        hero.ApplyEffect(new PoisonEffect(damagePerTurn: 8));
        hero.ApplyEffect(new BurnEffect(damagePerTurn: 15));
        hero.ApplyEffect(new FreezeEffect());


        hero.ApplyEffect(new PoisonEffect(damagePerTurn: 8));
        hero.ApplyEffect(new BurnEffect(damagePerTurn: 15));

        hero.PrintStatus();


        for (int turn = 1; turn <= 4; turn++)
        {
            hero.ProcessTurn();
            hero.PrintStatus();

            if (turn == 2)
            {
                Console.WriteLine();
                hero.ApplyEffect(new RegenerationEffect(healPerTurn: 10));
            }
        }
    }
}
