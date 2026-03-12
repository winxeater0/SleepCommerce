namespace SleepCommerce.Domain.Entities;

public class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Estoque { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    private Produto() { }

    public Produto(string nome, string? descricao, int estoque, decimal valor)
    {
        Id = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
        SetNome(nome);
        Descricao = descricao;
        SetEstoque(estoque);
        SetValor(valor);
    }

    public static Produto Reconstituir(Guid id, string nome, string? descricao, int estoque, decimal valor, DateTime dataCriacao, DateTime? dataAtualizacao)
    {
        var produto = new Produto();
        produto.Id = id;
        produto.SetNome(nome);
        produto.Descricao = descricao;
        produto.SetEstoque(estoque);
        produto.SetValor(valor);
        produto.DataCriacao = dataCriacao;
        produto.DataAtualizacao = dataAtualizacao;
        return produto;
    }

    public void Atualizar(string nome, string? descricao, int estoque, decimal valor)
    {
        SetNome(nome);
        Descricao = descricao;
        SetEstoque(estoque);
        SetValor(valor);
        DataAtualizacao = DateTime.UtcNow;
    }

    private void SetNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        if (nome.Length > 200)
            throw new ArgumentException("Nome deve ter no máximo 200 caracteres.", nameof(nome));

        Nome = nome;
    }

    private void SetEstoque(int estoque)
    {
        if (estoque < 0)
            throw new ArgumentException("Estoque não pode ser negativo.", nameof(estoque));

        Estoque = estoque;
    }

    private void SetValor(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        Valor = valor;
    }
}
