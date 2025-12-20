using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System;

// ===== ФОРМЫ КЛИЕНТОВ =====
public class ClientsForm : Form
{
    private DataGridView clientsGrid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn;
    public ClientsForm()
    {
        this.Text = "Клиенты";
        this.Width = 600;
        this.Height = 400;
        clientsGrid = new DataGridView { Dock = DockStyle.Top, Height = 300, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        addBtn = new Button { Text = "Добавить", Left = 10, Top = 320 };
        editBtn = new Button { Text = "Изменить", Left = 110, Top = 320 };
        deleteBtn = new Button { Text = "Удалить", Left = 210, Top = 320 };
        refreshBtn = new Button { Text = "Обновить", Left = 310, Top = 320 };
        addBtn.Click += AddBtn_Click;
        editBtn.Click += EditBtn_Click;
        deleteBtn.Click += DeleteBtn_Click;
        refreshBtn.Click += (s,e) => LoadClients();
        this.Controls.Add(clientsGrid);
        this.Controls.Add(addBtn);
        this.Controls.Add(editBtn);
        this.Controls.Add(deleteBtn);
        this.Controls.Add(refreshBtn);
        LoadClients();
    }
    private void LoadClients()
    {
        clientsGrid.DataSource = null;
        clientsGrid.DataSource = DataRepository.Clients.Select(c => new { c.Id, c.FullName, c.Contacts, Заказов = c.Orders.Count }).ToList();
    }
    private Client GetSelectedClient()
    {
        if (clientsGrid.SelectedRows.Count != 1) return null;
        var idProp = clientsGrid.SelectedRows[0].DataBoundItem?.GetType().GetProperty("Id");
        if (idProp == null) return null;
        int cid = (int)idProp.GetValue(clientsGrid.SelectedRows[0].DataBoundItem);
        return DataRepository.Clients.FirstOrDefault(x => x.Id == cid);
    }
    private void AddBtn_Click(object sender, EventArgs e)
    {
        var dlg = new ClientEditForm();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            DataRepository.Clients.Add(new Client { Id = GetNextClientId(), FullName = dlg.FullName, Contacts = dlg.Contacts });
            DataRepository.SaveAll();
            LoadClients();
        }
    }
    private int GetNextClientId() => DataRepository.Clients.Any() ? DataRepository.Clients.Max(c => c.Id) + 1 : 1;
    private void EditBtn_Click(object sender, EventArgs e)
    {
        var cl = GetSelectedClient();
        if (cl == null) { MessageBox.Show("Не выбран клиент!"); return; }
        var dlg = new ClientEditForm(cl.FullName) { Contacts = cl.Contacts };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            cl.FullName = dlg.FullName;
            cl.Contacts = dlg.Contacts;
            DataRepository.SaveAll();
            LoadClients();
        }
    }
    private void DeleteBtn_Click(object sender, EventArgs e)
    {
        var cl = GetSelectedClient();
        if (cl == null) { MessageBox.Show("Не выбран клиент!"); return; }
        if (MessageBox.Show($"Удалить клиента '{cl.FullName}'?", "Подтвердите", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            DataRepository.Clients.Remove(cl);
            DataRepository.SaveAll();
            LoadClients();
        }
    }
}

public class ClientEditForm : Form
{
    public string FullName { get => nameBox.Text; set => nameBox.Text = value; }
    public string Contacts { get => contactsBox.Text; set => contactsBox.Text = value; }
    TextBox nameBox, contactsBox;
    public ClientEditForm(string name = "")
    {
        this.Text = "Клиент";
        this.Width = 350;
        this.Height = 200;
        Label nameLbl = new Label { Text = "ФИО", Left = 10, Top = 20, Width = 80 };
        nameBox = new TextBox { Left = 100, Top = 20, Width = 200, Text = name };
        Label contLbl = new Label { Text = "Контакты", Left = 10, Top = 60, Width = 80 };
        contactsBox = new TextBox { Left = 100, Top = 60, Width = 200 };
        Button okBtn = new Button { Text = "OK", Left = 60, Top = 110, Width = 80 };
        Button cancelBtn = new Button { Text = "Отмена", Left = 180, Top = 110, Width = 80 };
        okBtn.Click += (s,e) => { this.DialogResult = DialogResult.OK; this.Close(); };
        cancelBtn.Click += (s,e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        this.Controls.Add(nameLbl); this.Controls.Add(nameBox);
        this.Controls.Add(contLbl); this.Controls.Add(contactsBox);
        this.Controls.Add(okBtn); this.Controls.Add(cancelBtn);
    }
}

// ===== ФОРМЫ ЗАКАЗОВ =====
public class OrdersForm : Form
{
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn, detailsBtn, toProdBtn, shipBtn;
    public OrdersForm()
    {
        this.Text = "Заказы";
        this.Width = 900;
        this.Height = 480;
        grid = new DataGridView { Dock = DockStyle.Top, Height = 300, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        addBtn = new Button { Text = "Добавить", Left = 10, Top = 320 };
        editBtn = new Button { Text = "Изменить", Left = 110, Top = 320 };
        deleteBtn = new Button { Text = "Удалить", Left = 210, Top = 320 };
        refreshBtn = new Button { Text = "Обновить", Left = 310, Top = 320 };
        detailsBtn = new Button { Text = "Карточка", Left = 410, Top = 320 };
        toProdBtn = new Button { Text = "Производство", Left = 510,Width = 100, Top = 320 };
        shipBtn = new Button { Text = "Отгрузить", Left = 650, Top = 320 };
        addBtn.Click += AddBtn_Click;
        editBtn.Click += EditBtn_Click;
        deleteBtn.Click += DeleteBtn_Click;
        refreshBtn.Click += (s,e) => LoadOrders();
        detailsBtn.Click += DetailsBtn_Click;
        toProdBtn.Click += ToProdBtn_Click;
        shipBtn.Click += ShipBtn_Click;
        this.Controls.Add(grid);
        this.Controls.Add(addBtn);
        this.Controls.Add(editBtn);
        this.Controls.Add(deleteBtn);
        this.Controls.Add(refreshBtn);
        this.Controls.Add(detailsBtn);
        this.Controls.Add(toProdBtn);
        this.Controls.Add(shipBtn);
        LoadOrders();
    }
    private object SelectedOrderObj => grid.SelectedRows.Count == 1 ? grid.SelectedRows[0].DataBoundItem : null;
    private Order GetSelectedOrder()
    {
        var idProp = SelectedOrderObj?.GetType().GetProperty("Id");
        if (idProp == null) return null;
        int orderId = (int)idProp.GetValue(SelectedOrderObj);
        return DataRepository.Orders.FirstOrDefault(x => x.Id == orderId);
    }
    private void LoadOrders()
    {
        grid.DataSource = null;
        grid.DataSource = DataRepository.Orders.Select(o => new { o.Id, Клиент = o.Client?.FullName, Менеджер = o.Manager?.FullName, o.Status, o.Amount, Позиций = o.Items.Count }).ToList();
    }
    private void AddBtn_Click(object sender, EventArgs e)
    {
        var dlg = new OrderEditForm();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var nextId = DataRepository.Orders.Any() ? DataRepository.Orders.Max(o => o.Id) + 1 : 1;
            var ord = new Order { Id = nextId, Client = dlg.SelectedClient, Manager = dlg.SelectedManager, Items = dlg.Items.ToList(), Status = "Создан", History = new List<OrderHistory> { new OrderHistory { DateTime = DateTime.Now, Status = "Создан", Comment = "Создание заказа" } } };
            ord.RecalculateAmount();
            DataRepository.Orders.Add(ord);
            ord.Client?.Orders.Add(ord);
            DataRepository.SaveAll();
            LoadOrders();
        }
    }
    private void EditBtn_Click(object sender, EventArgs e)
    {
        var ord = GetSelectedOrder();
        if (ord == null) { MessageBox.Show("Не выбран заказ!"); return; }
        if (ord.Status != "Создан") { MessageBox.Show("Изменять можно только заказы в статусе 'Создан'"); return; }
        var dlg = new OrderEditForm(ord.Client, ord.Manager, ord.Items);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            ord.Client = dlg.SelectedClient;
            ord.Manager = dlg.SelectedManager;
            ord.Items = dlg.Items.ToList();
            ord.RecalculateAmount();
            DataRepository.SaveAll();
            LoadOrders();
        }
    }
    private void DeleteBtn_Click(object sender, EventArgs e)
    {
        var ord = GetSelectedOrder();
        if (ord == null) { MessageBox.Show("Не выбран заказ!"); return; }
        if (MessageBox.Show($"Удалить заказ #{ord.Id}?", "Подтвердите", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            DataRepository.Orders.Remove(ord);
            ord.Client?.Orders.Remove(ord);
            DataRepository.SaveAll();
            LoadOrders();
        }
    }
    private void DetailsBtn_Click(object sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) { MessageBox.Show("Заказ не выбран"); return; }
        new OrderDetailsForm(order).ShowDialog();
    }
    private void ToProdBtn_Click(object sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) { MessageBox.Show("Заказ не выбран"); return; }
        if (order.Status != "Создан") { MessageBox.Show("Только заказы со статусом \"Создан\"! "); return; }
        order.Status = "В производстве";
        order.History.Add(new OrderHistory{ DateTime = DateTime.Now, Status = order.Status, Comment = "Передан в производство" });
        DataRepository.Orders.Single(x => x.Id == order.Id).Status = order.Status;
        DataRepository.SaveAll();
        MessageBox.Show("Заказ передан в производство (сценарий выполнен)");
        LoadOrders();
    }
    private void ShipBtn_Click(object sender, EventArgs e)
    {
        var order = GetSelectedOrder();
        if (order == null) { MessageBox.Show("Заказ не выбран"); return; }
        if (order.Status != "В производстве") { MessageBox.Show("Только после производства!"); return; }
        foreach(var itm in order.Items)
        {
            if(itm.Item is Material mat)
            {
                var mObj = DataRepository.Materials.FirstOrDefault(m => m.Id == mat.Id);
                if (mObj != null) mObj.Quantity -= itm.Quantity;
            }
        }
        order.Status = "Отгружено";
        order.History.Add(new OrderHistory{ DateTime = DateTime.Now, Status = order.Status, Comment = "Заказ отгружен" });
        DataRepository.Orders.Single(x => x.Id == order.Id).Status = order.Status;
        DataRepository.SaveAll();
        MessageBox.Show("Заказ отгружен (остатки обновлены)");
        LoadOrders();
    }
}

public class OrderEditForm : Form
{
    public Client SelectedClient => (Client)clientsBox.SelectedItem;
    public Employee SelectedManager => (Employee)managersBox.SelectedItem;
    private ListBox prodBox, matBox, orderBox;
    private TextBox prodQtyBox, matQtyBox;
    private Button addProdBtn, addMatBtn, delBtn, okBtn, cancelBtn;
    private List<OrderItem> items = new();
    private ComboBox clientsBox, managersBox;
    public OrderEditForm(Client client = null, Employee manager = null, List<OrderItem> existingItems = null)
    {
        this.Text = "Заказ";
        this.Width = 720;
        this.Height = 460;
        Label label1 = new Label { Text = "Клиент", Left = 10, Top = 20, Width = 100 };
        clientsBox = new ComboBox { Left = 120, Top = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        clientsBox.DataSource = DataRepository.Clients.ToList();
        clientsBox.DisplayMember = "FullName";
        if (client != null) clientsBox.SelectedItem = client;
        Label label2 = new Label { Text = "Менеджер", Left = 10, Top = 55, Width = 100 };
        managersBox = new ComboBox { Left = 120, Top = 55, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        managersBox.DataSource = DataRepository.Employees.ToList();
        managersBox.DisplayMember = "FullName";
        if (!DataRepository.Employees.Any())
        {
            var baseId = 0;
            var created = DataRepository.Users
                .Where(u => IsManagerOrAdmin(u))
                .Select((u, i) => new Employee { Id = baseId + i + 1, FullName = u.FullName, Position = u.Role, Contacts = u.Login })
                .ToList();
            if (created.Any())
            {
                DataRepository.Employees.AddRange(created);
                DataRepository.SaveAll();
                managersBox.DataSource = DataRepository.Employees.ToList();
            }
        }
        if (manager != null) managersBox.SelectedItem = manager;
        Label label3 = new Label { Text = "Товары", Left = 10, Top = 90, Width = 100 };
        prodBox = new ListBox { Left = 120, Top = 90, Width = 200, Height = 90 };
        prodBox.DataSource = DataRepository.Products.ToList();
        prodBox.DisplayMember = "Description";
        prodQtyBox = new TextBox { Left = 330, Top = 120, Width = 50, Text = "1" };
        addProdBtn = new Button { Text = "Добавить товар", Left = 390, Top = 118, Width = 140 };
        addProdBtn.Click += AddProdBtn_Click;

        Label label4 = new Label { Text = "Материалы", Left = 10, Top = 200, Width = 100 };
        matBox = new ListBox { Left = 120, Top = 200, Width = 200, Height = 90 };
        matBox.DataSource = DataRepository.Materials.ToList();
        matBox.DisplayMember = "Article";
        matQtyBox = new TextBox { Left = 330, Top = 230, Width = 50, Text = "1" };
        addMatBtn = new Button { Text = "Добавить материал", Left = 390, Top = 228, Width = 140 };
        addMatBtn.Click += AddMatBtn_Click;

        orderBox = new ListBox { Left = 10, Top = 300, Width = 680, Height = 80 };
        delBtn = new Button { Text = "Удалить позицию", Left = 260, Top = 385, Width = 140 };
        delBtn.Click += (s,e)=> { if(orderBox.SelectedIndex!=-1) { items.RemoveAt(orderBox.SelectedIndex); FillOrderBox(); } };
        okBtn = new Button { Text = "OK", Left = 430, Top = 385, Width = 80 };
        cancelBtn = new Button { Text = "Отмена", Left = 520, Top = 385, Width = 80 };
        okBtn.Click += (s,e) => { this.DialogResult = DialogResult.OK; this.Close(); };
        cancelBtn.Click += (s,e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        this.Controls.Add(label1); this.Controls.Add(clientsBox);
        this.Controls.Add(label2); this.Controls.Add(managersBox);
        this.Controls.Add(label3); this.Controls.Add(prodBox);
        this.Controls.Add(prodQtyBox); this.Controls.Add(addProdBtn);
        this.Controls.Add(label4); this.Controls.Add(matBox);
        this.Controls.Add(matQtyBox); this.Controls.Add(addMatBtn);
        this.Controls.Add(orderBox); this.Controls.Add(delBtn);
        this.Controls.Add(okBtn); this.Controls.Add(cancelBtn);

        if(existingItems!=null) { items = new List<OrderItem>(existingItems.Select(x=>new OrderItem{ Id=x.Id, Item=x.Item, Quantity=x.Quantity, Price=x.Price })); }
        FillOrderBox();
    }
    private void AddProdBtn_Click(object sender, EventArgs e)
    {
        if (prodBox.SelectedItem == null) { MessageBox.Show("Выберите товар"); return; }
        int qty = int.TryParse(prodQtyBox.Text, out var q) ? Math.Max(1,q) : 1;
        var p = (Product)prodBox.SelectedItem;
        items.Add(new OrderItem { Id = items.Count + 1, Item = p, Quantity = qty, Price = p.Price });
        FillOrderBox();
    }
    private void AddMatBtn_Click(object sender, EventArgs e)
    {
        if (matBox.SelectedItem == null) { MessageBox.Show("Выберите материал"); return; }
        int qty = int.TryParse(matQtyBox.Text, out var q) ? Math.Max(1,q) : 1;
        var m = (Material)matBox.SelectedItem;
        items.Add(new OrderItem { Id = items.Count + 100, Item = m, Quantity = qty, Price = m.Price });
        FillOrderBox();
    }
    private void FillOrderBox()
    {
        orderBox.DataSource = null;
        orderBox.DataSource = items.Select(i=> (i.Item is Product p ? $"Товар: {p.Description}" : $"Материал: {((Material)i.Item).Article}") + $", кол-во: {i.Quantity}, цена: {i.Price}").ToList();
    }
    public List<OrderItem> Items => items;
}

public class OrderDetailsForm : Form
{
    public OrderDetailsForm(Order order)
    {
        this.Text = $"Заказ №{order.Id}";
        this.Width = 800;
        this.Height = 500;
        var lbl = new Label { AutoSize = true, Text = $"Статус: {order.Status}\nКлиент: {order.Client?.FullName}\nМенеджер: {order.Manager?.FullName}", Left = 10, Top = 10 };

        // Контейнер для таблиц, чтобы не накладывались
        var panel = new TableLayoutPanel
        {
            Left = 10,
            Top = 60,
            Width = this.ClientSize.Width - 20,
            Height = this.ClientSize.Height - 80,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ColumnCount = 1,
            RowCount = 2,
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var itemsGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        itemsGrid.DataSource = order.Items.Select(i => new {
            Товар = i.Item is Product ? ((Product)i.Item).Description : ((Material)i.Item).Article,
            Тип = i.Item is Product ? "Товар" : "Материал",
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();

        var histGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        histGrid.DataSource = order.History.Select(h => new { h.DateTime, h.Status, h.Comment }).ToList();

        panel.Controls.Add(itemsGrid, 0, 0);
        panel.Controls.Add(histGrid, 0, 1);

        this.Controls.Add(lbl);
        this.Controls.Add(panel);
    }
}

// ===== ФОРМЫ СКЛАДА =====
public class InventoryForm : Form
{
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn;
    private ComboBox typeBox;
    private bool IsProductsMode => typeBox.SelectedIndex == 0;
    public InventoryForm()
    {
        this.Text = "Склад: товары и материалы";
        this.Width = 700;
        this.Height = 400;
        typeBox = new ComboBox { Left = 10, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        typeBox.Items.AddRange(new object[] { "Товары", "Материалы" });
        typeBox.SelectedIndex = 0;
        typeBox.SelectedIndexChanged += (s, e) => LoadItems();
        grid = new DataGridView { Left = 10, Top = 40, Width = 660, Height = 260, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        addBtn = new Button { Text = "Добавить", Left = 10, Top = 320 };
        editBtn = new Button { Text = "Изменить", Left = 110, Top = 320 };
        deleteBtn = new Button { Text = "Удалить", Left = 210, Top = 320 };
        refreshBtn = new Button { Text = "Обновить", Left = 310, Top = 320 };
        addBtn.Click += AddBtn_Click;
        editBtn.Click += EditBtn_Click;
        deleteBtn.Click += DeleteBtn_Click;
        refreshBtn.Click += (s, e) => LoadItems();
        this.Controls.Add(typeBox);
        this.Controls.Add(grid);
        this.Controls.Add(addBtn);
        this.Controls.Add(editBtn);
        this.Controls.Add(deleteBtn);
        this.Controls.Add(refreshBtn);
        LoadItems();
    }
    private void LoadItems() {
        grid.DataSource = null;
        if (IsProductsMode) {
            grid.DataSource = DataRepository.Products.Select(p => new { p.Id, p.Article, p.Description, p.Price }).ToList();
        } else {
            grid.DataSource = DataRepository.Materials.Select(m => new { m.Id, m.Article, m.Quantity, m.Price }).ToList();
        }
    }
    private void AddBtn_Click(object sender, EventArgs e) {
        if (IsProductsMode) {
            var dlg = new ProductEditForm();
            if (dlg.ShowDialog() == DialogResult.OK) {
                var nextId = DataRepository.Products.Any() ? DataRepository.Products.Max(x => x.Id) + 1 : 1;
                DataRepository.Products.Add(new Product { Id = nextId, Article = dlg.Article, Description = dlg.Description, Price = dlg.Price });
            } else return;
        } else {
            var dlg = new MaterialEditForm();
            if (dlg.ShowDialog() == DialogResult.OK) {
                var nextId = DataRepository.Materials.Any() ? DataRepository.Materials.Max(x => x.Id) + 1 : 1;
                DataRepository.Materials.Add(new Material { Id = nextId, Article = dlg.Article, Quantity = dlg.Quantity, Price = dlg.Price });
            } else return;
        }
        DataRepository.SaveAll();
        LoadItems();
    }
    private object GetSelectedItem() {
        if (grid.SelectedRows.Count != 1) return null;
        return grid.SelectedRows[0].DataBoundItem;
    }
    private void EditBtn_Click(object sender, EventArgs e) {
        var obj = GetSelectedItem();
        if (obj == null) { MessageBox.Show("Не выбрано!"); return; }
        int id = (int)obj.GetType().GetProperty("Id").GetValue(obj);
        bool updated = false;

        if (IsProductsMode) {
            var prod = DataRepository.Products.First(x => x.Id == id);
            var dlg = new ProductEditForm(prod.Article, prod.Description, prod.Price);
            if (dlg.ShowDialog() == DialogResult.OK) {
                prod.Article = dlg.Article;
                prod.Description = dlg.Description;
                prod.Price = dlg.Price;
                updated = true;
            }
        } else {
            var mat = DataRepository.Materials.First(x => x.Id == id);
            var dlg = new MaterialEditForm(mat.Article, mat.Quantity, mat.Price);
            if (dlg.ShowDialog() == DialogResult.OK) {
                mat.Article = dlg.Article;
                mat.Quantity = dlg.Quantity;
                mat.Price = dlg.Price;
                updated = true;
            }
        }
        if (updated) {
            DataRepository.SaveAll();
            LoadItems();
        }
    }
    private void DeleteBtn_Click(object sender, EventArgs e) {
        var obj = GetSelectedItem();
        if (obj == null) { MessageBox.Show("Не выбрано!"); return; }
        int id = (int)obj.GetType().GetProperty("Id").GetValue(obj);
        bool confirmed = false;

        if (IsProductsMode) {
            var prod = DataRepository.Products.First(x => x.Id == id);
            if (MessageBox.Show($"Удалить товар {prod.Description}?","Удалить",MessageBoxButtons.YesNo)==DialogResult.Yes) {
                DataRepository.Products.Remove(prod);
                confirmed = true;
            }
        } else {
            var mat = DataRepository.Materials.First(x => x.Id == id);
            if (MessageBox.Show($"Удалить материал {mat.Article}?","Удалить",MessageBoxButtons.YesNo)==DialogResult.Yes) {
                DataRepository.Materials.Remove(mat);
                confirmed = true;
            }
        }

        if (confirmed) {
            DataRepository.SaveAll();
            LoadItems();
        }
    }
}

// Базовый класс для форм редактирования (Pull Up Field, Pull Up Method)
public abstract class BaseEditForm : Form {
    protected TextBox artBox, priceBox;  // Pull Up Field: общие поля вынесены в базовый класс
    
    public string Article => artBox.Text;
    public decimal Price => decimal.TryParse(priceBox.Text, out var v) ? v : 0;
    
    // Pull Up Method: общий метод добавления кнопок OK/Отмена
    protected void AddOkCancelButtons(int topPosition) {
        Button okBtn = new Button { Text = "OK", Left = 60, Top = topPosition, Width = 80 };
        Button cancelBtn = new Button { Text = "Отмена", Left = 180, Top = topPosition, Width = 80 };
        okBtn.Click += (s,e) => { this.DialogResult=DialogResult.OK; this.Close(); };
        cancelBtn.Click += (s,e) => { this.DialogResult=DialogResult.Cancel; this.Close(); };
        this.Controls.Add(okBtn);
        this.Controls.Add(cancelBtn);
    }
    
    // Pull Up Method: общий метод добавления поля "Артикул"
    protected void AddArticleField(string article) {
        Label l1 = new Label{Text="Артикул",Left=10,Top=20,Width=80};
        artBox = new TextBox{Left=100,Top=20,Width=200,Text=article};
        this.Controls.Add(l1);
        this.Controls.Add(artBox);
    }
    
    // Pull Up Method: общий метод добавления поля "Цена"
    protected void AddPriceField(int topPosition, decimal price) {
        Label l3 = new Label{Text="Цена",Left=10,Top=topPosition,Width=80};
        priceBox = new TextBox{Left=100,Top=topPosition,Width=200,Text=price.ToString()};
        this.Controls.Add(l3);
        this.Controls.Add(priceBox);
    }
}

public class ProductEditForm : BaseEditForm {
    public string Description => descBox.Text;
    TextBox descBox;  // Push Down Field: специфичное поле осталось в подклассе
    public ProductEditForm(string article = "", string description = "", decimal price = 0) {
        this.Text = "Товар";
        this.Width = 350;
        this.Height = 220;
        AddArticleField(article);  // Использование метода из базового класса
        Label l2 = new Label{Text="Описание",Left=10,Top=60,Width=80};
        descBox = new TextBox{Left=100,Top=60,Width=200,Text=description};
        this.Controls.Add(l2);
        this.Controls.Add(descBox);
        AddPriceField(100, price);  // Использование метода из базового класса
        AddOkCancelButtons(150);  // Использование метода из базового класса
    }
}

public class MaterialEditForm : BaseEditForm {
    public int Quantity => int.TryParse(quantityBox.Text, out var v) ? v : 0;
    TextBox quantityBox;  // Push Down Field: специфичное поле осталось в подклассе
    public MaterialEditForm(string article = "", int quantity = 0, decimal price = 0) {
        this.Text = "Материал";
        this.Width = 350;
        this.Height = 250;
        AddArticleField(article);  // Использование метода из базового класса
        Label l2 = new Label{Text="Остаток",Left=10,Top=60,Width=80};
        quantityBox = new TextBox{Left=100,Top=60,Width=200,Text=quantity.ToString()};
        this.Controls.Add(l2);
        this.Controls.Add(quantityBox);
        AddPriceField(100, price);  // Использование метода из базового класса
        AddOkCancelButtons(180);  // Использование метода из базового класса
    }
}

// ===== ФОРМЫ ПРОИЗВОДСТВА =====
public class ProductionForm : Form
{
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn;
    static List<ProductionTask> Tasks = new();
    public ProductionForm()
    {
        this.Text = "Производственные задачи";
        this.Width = 750;
        this.Height = 450;
        grid = new DataGridView { Dock = DockStyle.Top, Height = 300, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        addBtn = new Button { Text = "Добавить", Left = 10, Top = 320 };
        editBtn = new Button { Text = "Изменить", Left = 110, Top = 320 };
        deleteBtn = new Button { Text = "Удалить", Left = 210, Top = 320 };
        refreshBtn = new Button { Text = "Обновить", Left = 310, Top = 320 };
        addBtn.Click += AddBtn_Click;
        editBtn.Click += EditBtn_Click;
        deleteBtn.Click += DeleteBtn_Click;
        refreshBtn.Click += (s,e) => LoadTasks();
        this.Controls.Add(grid);
        this.Controls.Add(addBtn);
        this.Controls.Add(editBtn);
        this.Controls.Add(deleteBtn);
        this.Controls.Add(refreshBtn);
        if(!Tasks.Any() && DataRepository.Orders.Any(o=>o.Status=="В производстве"))
        {
            int pid = 1;
            foreach(var o in DataRepository.Orders.Where(x=>x.Status=="В производстве"))
                Tasks.Add(new ProductionTask { Id = pid++, Order = o, Workshop = "Цех 1", Stage = "Старт", Status = "В работе" });
        }
        LoadTasks();
    }
    private void LoadTasks()
    {
        grid.DataSource = null;
        grid.DataSource = Tasks.Select(t => new {
            t.Id,
            OrderId = t.Order?.Id,
            t.Workshop,
            t.Stage,
            t.Status,
            Клиент = t.Order?.Client?.FullName
        }).ToList();
    }
    private ProductionTask GetSelected()
    {
        if(grid.SelectedRows.Count!=1) return null;
        var idProp = grid.SelectedRows[0].DataBoundItem?.GetType().GetProperty("Id");
        if (idProp == null) return null;
        int tid = (int)idProp.GetValue(grid.SelectedRows[0].DataBoundItem);
        return Tasks.FirstOrDefault(x=>x.Id==tid);
    }
    private void AddBtn_Click(object sender, EventArgs e)
    {
        var dlg = new ProductionTaskEditForm();
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var nextId = Tasks.Any() ? Tasks.Max(x=>x.Id)+1 : 1;
            Tasks.Add(new ProductionTask {
                Id = nextId,
                Order = dlg.SelectedOrder,
                Workshop = dlg.Workshop,
                Stage = dlg.Stage,
                Status = dlg.Status
            });
            LoadTasks();
        }
    }
    private void EditBtn_Click(object sender, EventArgs e)
    {
        var p = GetSelected();
        if (p == null) { MessageBox.Show("Не выбрано!"); return; }
        var dlg = new ProductionTaskEditForm(p.Order, p.Workshop, p.Stage, p.Status);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            p.Order = dlg.SelectedOrder;
            p.Workshop = dlg.Workshop;
            p.Stage = dlg.Stage;
            p.Status = dlg.Status;
            LoadTasks();
        }
    }
    private void DeleteBtn_Click(object sender, EventArgs e)
    {
        var p = GetSelected();
        if (p == null) { MessageBox.Show("Не выбрано!"); return; }
        if (MessageBox.Show($"Удалить задачу №{p.Id}?","Удалить",MessageBoxButtons.YesNo)==DialogResult.Yes)
        {
            Tasks.Remove(p);
            LoadTasks();
        }
    }
}

public class ProductionTaskEditForm : Form
{
    public Order SelectedOrder => (Order)orderBox.SelectedItem;
    public string Workshop => workBox.Text;
    public string Stage => stageBox.Text;
    public string Status => statusBox.Text;
    ComboBox orderBox;
    TextBox workBox, stageBox, statusBox;
    public ProductionTaskEditForm(Order order = null, string workshop = "", string stage = "", string status = "")
    {
        this.Text = "Производственная задача";
        this.Width = 470;
        this.Height = 290;
        Label l1 = new Label{Text="Заказ",Left=10,Top=20,Width=100};
        orderBox = new ComboBox{Left=120,Top=20,Width=300,DropDownStyle=ComboBoxStyle.DropDownList};
        orderBox.DataSource = DataRepository.Orders.Where(o=>o.Status!="Отгружено").ToList();
        orderBox.DisplayMember = "Id";
        if(order!=null) orderBox.SelectedItem=order;
        Label l2 = new Label{Text="Цех",Left=10,Top=60,Width=100};
        workBox = new TextBox{Left=120,Top=60,Width=300,Text=workshop};
        Label l3 = new Label{Text="Этап",Left=10,Top=100,Width=100};
        stageBox = new TextBox{Left=120,Top=100,Width=300,Text=stage};
        Label l4 = new Label{Text="Статус",Left=10,Top=140,Width=100};
        statusBox = new TextBox{Left=120,Top=140,Width=300,Text=status};
        Button okBtn = new Button { Text = "OK", Left = 140, Top = 200, Width = 80 };
        Button cancelBtn = new Button { Text = "Отмена", Left = 270, Top = 200, Width = 80 };
        okBtn.Click += (s,e) => { this.DialogResult=DialogResult.OK;this.Close(); };
        cancelBtn.Click += (s,e) => { this.DialogResult=DialogResult.Cancel;this.Close(); };
        this.Controls.Add(l1);this.Controls.Add(orderBox);
        this.Controls.Add(l2);this.Controls.Add(workBox);
        this.Controls.Add(l3);this.Controls.Add(stageBox);
        this.Controls.Add(l4);this.Controls.Add(statusBox);
        this.Controls.Add(okBtn);this.Controls.Add(cancelBtn);
    }
}

// ===== ФОРМЫ ОТЧЁТОВ =====
public class ReportsForm : Form
{
    private DataGridView grid;
    private ComboBox filterBox;
    private Button refreshBtn;
    public ReportsForm()
    {
        this.Text = "Отчеты";
        this.Width = 700;
        this.Height = 400;
        filterBox = new ComboBox { Left = 10, Top = 10, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        filterBox.Items.AddRange(new object[] {
            "Динамика продаж по статусу заказа",
            "Выполнение по менеджерам",
            "Оборотная ведомость (клиенты)",
        });
        filterBox.SelectedIndex = 0;
        grid = new DataGridView { Left = 10, Top = 50, Width = 650, Height = 280, ReadOnly = true, AutoGenerateColumns = true };
        refreshBtn = new Button { Text = "Построить", Left = 330, Top = 10 };
        refreshBtn.Click += (s,e) => BuildReport();
        this.Controls.Add(filterBox);
        this.Controls.Add(grid);
        this.Controls.Add(refreshBtn);
        BuildReport();
    }
    private void BuildReport()
    {
        grid.DataSource = null;
        switch(filterBox.SelectedIndex)
        {
            case 0:
                grid.DataSource = DataRepository.Orders.GroupBy(o => o.Status)
                    .Select(g => new { Статус = g.Key, Заказов = g.Count(), ОбщаяСумма = g.Sum(x => x.Amount) })
                    .ToList();
                break;
            case 1:
                grid.DataSource = DataRepository.Orders.GroupBy(o => o.Manager?.FullName)
                    .Select(g => new { Менеджер = g.Key, Заказов = g.Count(), ОбщаяСумма = g.Sum(x => x.Amount) })
                    .ToList();
                break;
            case 2:
                grid.DataSource = DataRepository.Orders.GroupBy(o => o.Client?.FullName)
                    .Select(g => new { Клиент = g.Key, Заказов = g.Count(), ОбщаяСумма = g.Sum(x => x.Amount) })
                    .ToList();
                break;
        }
    }
}

// ===== ФОРМЫ НАСТРОЕК =====
public class SettingsForm : Form
{
    private Button usersBtn, rolesBtn, integrationBtn;
    public SettingsForm()
    {
        this.Text = "Настройки";
        this.Width = 350;
        this.Height = 220;
        usersBtn = new Button { Text = "Пользователи", Left = 10, Top = 20, Width = 300 };
        rolesBtn = new Button { Text = "Роли", Left = 10, Top = 60, Width = 300 };
        integrationBtn = new Button { Text = "Интеграции (stub)", Left = 10, Top = 100, Width = 300 };
        usersBtn.Click += (s,e) => new UsersForm().ShowDialog();
        rolesBtn.Click += (s,e) => MessageBox.Show("Справочник ролей: Администратор, Менеджер, Оператор.");
        integrationBtn.Click += (s,e) => MessageBox.Show("Интерфейс для будущей интеграции с 1С (заглушка)");
        this.Controls.Add(usersBtn);
        this.Controls.Add(rolesBtn);
        this.Controls.Add(integrationBtn);
    }
}

public class UsersForm : Form {
    private DataGridView grid;
    private Button addBtn, editBtn, deleteBtn, refreshBtn;
    public UsersForm(){
        this.Text="Пользователи";
        this.Width=650;
        this.Height=340;
        grid=new DataGridView{Dock=DockStyle.Top, Height=200, ReadOnly=true, AutoGenerateColumns=true, SelectionMode=DataGridViewSelectionMode.FullRowSelect};
        addBtn=new Button{Text="Добавить",Left=10,Top=220};
        editBtn=new Button{Text="Изменить",Left=110,Top=220};
        deleteBtn=new Button{Text="Удалить",Left=210,Top=220};
        refreshBtn=new Button{Text="Обновить",Left=310,Top=220};
        addBtn.Click+=AddBtn_Click;
        editBtn.Click+=EditBtn_Click;
        deleteBtn.Click+=DeleteBtn_Click;
        refreshBtn.Click+=(s,e)=>LoadUsers();
        this.Controls.Add(grid);
        this.Controls.Add(addBtn);
        this.Controls.Add(editBtn);
        this.Controls.Add(deleteBtn);
        this.Controls.Add(refreshBtn);
        LoadUsers();
    }
    private void LoadUsers(){
        grid.DataSource=null;
        grid.DataSource=DataRepository.Users.Select(u=>new{u.Login,u.FullName,u.Role}).ToList();
    }
    private User GetUser(){
        if(grid.SelectedRows.Count!=1) return null;
        string login=(string)grid.SelectedRows[0].Cells[0].Value;
        return DataRepository.Users.FirstOrDefault(x=>x.Login==login);
    }
    private void AddBtn_Click(object sender, EventArgs e){
        var dlg=new UserEditForm();
        if(dlg.ShowDialog()==DialogResult.OK){
            DataRepository.Users.Add(new User{Login=dlg.Login,Password=dlg.Password,FullName=dlg.FullName,Role=dlg.Role});
            DataRepository.SaveAll();
            LoadUsers();
        }
    }
    private void EditBtn_Click(object sender, EventArgs e){
        var user=GetUser();
        if(user==null){MessageBox.Show("Пользователь не выбран");return;}
        var dlg=new UserEditForm(user.Login,user.Password,user.FullName,user.Role,true);
        if(dlg.ShowDialog()==DialogResult.OK){
            user.Password=dlg.Password;user.FullName=dlg.FullName;user.Role=dlg.Role;
            DataRepository.SaveAll();
            LoadUsers();
        }
    }
    private void DeleteBtn_Click(object sender, EventArgs e){
        var user=GetUser();
        if(user==null){MessageBox.Show("Пользователь не выбран");return;}
        if(MessageBox.Show($"Удалить пользователя '{user.Login}'?","Подтвердите",MessageBoxButtons.YesNo)==DialogResult.Yes)
        {DataRepository.Users.Remove(user);DataRepository.SaveAll();LoadUsers();}
    }
}
public class UserEditForm : Form{
    public string Login => loginBox.Text;
    public string Password => passBox.Text;
    public string FullName => nameBox.Text;
    public string Role => roleBox.Text;
    TextBox loginBox, passBox, nameBox, roleBox;
    public UserEditForm(string login = "", string pass = "", string name = "", string role = "Менеджер", bool edit = false){
        this.Text=(edit?"Изменить":"Добавить")+" пользователя";
        this.Width=350;this.Height=250;
        Label l1=new Label{Text="Логин",Left=10,Top=20,Width=80};
        loginBox=new TextBox{Left=100,Top=20,Width=200,Text=login,ReadOnly=edit};
        Label l2=new Label{Text="Пароль",Left=10,Top=60,Width=80};
        passBox=new TextBox{Left=100,Top=60,Width=200,Text=pass};
        Label l3=new Label{Text="ФИО",Left=10,Top=100,Width=80};
        nameBox=new TextBox{Left=100,Top=100,Width=200,Text=name};
        Label l4=new Label{Text="Роль",Left=10,Top=140,Width=80};
        roleBox=new TextBox{Left=100,Top=140,Width=200,Text=role};
        Button okBtn=new Button{Text="OK",Left=60,Top=180,Width=80};
        Button cancelBtn=new Button{Text="Отмена",Left=180,Top=180,Width=80};
        okBtn.Click+=(s,e)=>{this.DialogResult=DialogResult.OK;this.Close();};
        cancelBtn.Click+=(s,e)=>{this.DialogResult=DialogResult.Cancel;this.Close();};
        this.Controls.Add(l1);this.Controls.Add(loginBox);
        this.Controls.Add(l2);this.Controls.Add(passBox);
        this.Controls.Add(l3);this.Controls.Add(nameBox);
        this.Controls.Add(l4);this.Controls.Add(roleBox);
        this.Controls.Add(okBtn);this.Controls.Add(cancelBtn);
    }
}

// ===== ФОРМЫ АВТОРИЗАЦИИ =====
public class LoginForm : Form
{
    TextBox loginBox, passBox;
    Label errorLbl;
    public User AuthUser { get; private set; }
    public LoginForm()
    {
        this.Text = "Авторизация";
        this.Width = 330;
        this.Height = 230;
        Label l1 = new Label { Text = "Логин", Left = 10, Top = 25, Width = 80 };
        loginBox = new TextBox { Left = 110, Top = 20, Width = 180 };
        Label l2 = new Label { Text = "Пароль", Left = 10, Top = 65, Width = 80 };
        passBox = new TextBox { Left = 110, Top = 60, Width = 180, PasswordChar = '*' };
        Button okBtn = new Button { Text = "Войти", Left = 50, Top = 110, Width = 100 };
        Button cancelBtn = new Button { Text = "Отмена", Left = 170, Top = 110, Width = 100 };
        okBtn.Click += OkBtn_Click;
        cancelBtn.Click += (s,e)=> { this.DialogResult = DialogResult.Cancel; this.Close(); };
        errorLbl = new Label { Text = "", ForeColor = System.Drawing.Color.Red, Left = 10, Top = 150, Width = 300 };
        this.Controls.Add(l1); this.Controls.Add(loginBox);
        this.Controls.Add(l2); this.Controls.Add(passBox);
        this.Controls.Add(okBtn); this.Controls.Add(cancelBtn);
        this.Controls.Add(errorLbl);
        this.AcceptButton = okBtn;
    }
    private void OkBtn_Click(object sender, EventArgs e)
    {
        var user = DataRepository.Users.FirstOrDefault(u => u.Login == loginBox.Text.Trim() && u.Password == passBox.Text);
        if(user==null)
        {
            errorLbl.Text = "Неверный логин или пароль";
            return;
        }
        AuthUser = user;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
