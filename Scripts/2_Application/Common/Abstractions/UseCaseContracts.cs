namespace Roguelike.Application.Abstractions
{
    /// <summary>
    /// アプリケーション層ユースケースの共通契約です。
    /// </summary>
    public interface IUseCase<in TRequest, TResponse>
    {
        Result<TResponse> Handle(TRequest request);
    }

    /// <summary>
    /// コマンド要求のマーカーです。
    /// </summary>
    public interface ICommand
    {
    }

    /// <summary>
    /// 結果を返すコマンド要求のマーカーです。
    /// </summary>
    public interface ICommand<TResult> : ICommand
    {
    }

    /// <summary>
    /// クエリ要求のマーカーです。
    /// </summary>
    public interface IQuery<TResult>
    {
    }

    /// <summary>
    /// 引数なし要求を表現する値です。
    /// </summary>
    public readonly struct Unit
    {
        public static readonly Unit Value = new Unit();
    }
}
