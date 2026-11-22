using System;
using System.Collections.Generic;
using System.Linq;
public abstract class StatusEffectBase : IStatusEffect
{
    public string Name { get; protected set; }
    public int RemainingTurns { get; protected set; }
    public bool IsStackable { get; protected set; }

    protected StatusEffectBase(string name, int duration, bool isStackable)
    {
        Name = name;
        RemainingTurns = duration;
        IsStackable = isStackable;
    }

    public abstract void OnTurnStart(Character target);

    public virtual void Merge(IStatusEffect newEffect)
    {
        if (IsStackable)
        {
            // Сложила длительность
            RemainingTurns += newEffect.RemainingTurns;
        }
        else
        {
            // Обновила длительность
            RemainingTurns = Math.Max(RemainingTurns, newEffect.RemainingTurns);
        }
    }

    public bool Tick()
    {
        RemainingTurns--;
        return RemainingTurns <= 0;
    }
}

// Создала класс персонажа
public class Character
{
    public string Name { get; }
    public int MaxHP { get; }
    public int HP { get; private set; }
    public int Armor { get; }
    public bool CanAct { get; set; } = true;

    private readonly List<IStatusEffect> _activeEffects = new();
    public IReadOnlyList<IStatusEffect> ActiveEffects => _activeEffects;

    public Character(string name, int maxHp, int armor)
    {
        Name = name;
        MaxHP = maxHp;
        HP = maxHp;
        Armor = armor;
    }

    public void TakeDamage(int amount, bool ignoreArmor = false)
    {
        int actualDamage = ignoreArmor ? amount : Math.Max(0, amount - Armor);
        HP = Math.Max(0, HP - actualDamage);
        string armorInfo = ignoreArmor ? "(игнор брони)" : $"(броня поглотила {amount - actualDamage})";
        Console.WriteLine($"  {Name} получает {actualDamage} урона {armorInfo}.");
    }

    public void Heal(int amount)
    {
        int oldHp = HP;
        HP = Math.Min(MaxHP, HP + amount);
        Console.WriteLine($"  {Name} восстанавливает {HP - oldHp} HP.");
    }

    // Наложила эффект
    public void ApplyEffect(IStatusEffect newEffect)
    {
        var existingEffect = _activeEffects.FirstOrDefault(e => e.GetType() == newEffect.GetType());

        if (existingEffect != null)
        {
            existingEffect.Merge(newEffect);
            string action = existingEffect.IsStackable ? "сложен (стак)" : "обновлён";
            Console.WriteLine($"Эффект: {newEffect.Name} на {Name} {action}. Осталось ходов: {existingEffect.RemainingTurns}");
        }
        else
        {
            _activeEffects.Add(newEffect);
            Console.WriteLine($"Эффект: на {Name} наложен {newEffect.Name} ({newEffect.RemainingTurns} ход.).");
        }
    }

    // Обработала начало хода
    public void ProcessTurn()
    {
        Console.WriteLine($"\nНачало хода персонажа {Name}:");
        
        // Сбросила флаг действия
        CanAct = true;

        // Прошла с конца для безопасного удаления
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.OnTurnStart(this);

            if (effect.Tick())
            {
                Console.WriteLine($"  Эффект '{effect.Name}' истёк и снят с {Name}.");
                _activeEffects.RemoveAt(i);
            }
        }

        if (CanAct)
            Console.WriteLine($"{Name} готов к действиям в этот ход!");
        else
            Console.WriteLine($"{Name} оглушён или заморожен и пропускает ход!");
    }

    public void PrintStatus()
    {
        string effectsList = _activeEffects.Count == 0
            ? "нет"
            : string.Join(", ", _activeEffects.Select(e => $"{e.Name} ({e.RemainingTurns} х.)"));

        Console.WriteLine($"Статус {Name}: HP {HP}/{MaxHP} | Броня: {Armor} | Активные эффекты: {effectsList}");
    }
}

// Реализовала отравление
public class PoisonEffect : StatusEffectBase
{
    private readonly int _damagePerTurn;

    public PoisonEffect(int damagePerTurn = 10, int duration = 3)
        : base("Отравление", duration, isStackable: true)
    {
        _damagePerTurn = damagePerTurn;
    }

    public override void OnTurnStart(Character target)
    {
        Console.WriteLine($"  Тик ({Name}): Яд разъедает тело...");
        target.TakeDamage(_damagePerTurn, ignoreArmor: true);
    }
}

// Реализовала горение
public class BurnEffect : StatusEffectBase
{
    private readonly int _damagePerTurn;

    public BurnEffect(int damagePerTurn = 15, int duration = 2)
        : base("Горение", duration, isStackable: false)
    {
        _damagePerTurn = damagePerTurn;
    }

    public override void OnTurnStart(Character target)
    {
        Console.WriteLine($"  Тик ({Name}): Пламя обжигает броню...");
        target.TakeDamage(_damagePerTurn, ignoreArmor: false);
    }
}

// Реализовала регенерацию
public class RegenerationEffect : StatusEffectBase
{
    private readonly int _healPerTurn;

    public RegenerationEffect(int healPerTurn = 12, int duration = 4)
        : base("Регенерация", duration, isStackable: false)
    {
        _healPerTurn = healPerTurn;
    }

    public override void OnTurnStart(Character target)
    {
        Console.WriteLine($"  Тик ({Name}): Раны затягиваются...");
        target.Heal(_healPerTurn);
    }
}

// Реализовала заморозку
public class FreezeEffect : StatusEffectBase
{
    public FreezeEffect(int duration = 1)
        : base("Заморозка", duration, isStackable: false)
    {
    }

    public override void OnTurnStart(Character target)
    {
        Console.WriteLine($"  Тик ({Name}): Персонаж скован льдом!");
        target.CanAct = false;
    }
}
