public interface IStatusEffect
{
    string Name { get; }
    int RemainingTurns { get; }
    bool IsStackable { get; }

    // Применила эффект
    void OnTurnStart(Character target);

    // Обработала повторное наложение
    void Merge(IStatusEffect newEffect);

    // Уменьшила длительность
    bool Tick();
}
