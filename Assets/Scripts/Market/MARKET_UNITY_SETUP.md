# Настройка маркета в Unity

Маркет состоит из двух частей:

- Базар - прямая покупка и продажа товаров через `Bazaar Profile`.
- Контракты - заказы от контрактных покупателей через `Contract Buyer Profile` и `Order Template`.

Базар покупает и продает напрямую. Контрактные покупатели не покупают товары напрямую: они только выдают контракты, а нужные товары задаются строками в шаблонах контрактов.

## 1. Папки

Рекомендуемая структура:

- `Assets/Market/Prefabs`
- `Assets/Market/Data/Items`
- `Assets/Market/Data/Buyers`
- `Assets/Market/Data/Orders`
- `Assets/Market/Sprites`

## 2. Товары

Создай товары через Project window:

- `Create -> Items -> Sellable Item`
- `Create -> Items -> Seed Item`
- `Create -> Items -> Produce Item`

Минимальный набор для теста:

- `Seed_Tomato`
- `Seed_Carrot`
- `Produce_Tomato`
- `Produce_Carrot`

Заполни у каждого товара:

- `ID` - уникальное число, например `101`, `102`, `201`, `202`
- `ItemName` - имя для UI
- `Icon` - спрайт товара
- `Stackable` - обычно `true`
- `MaxStack` - например `99`
- `Price` - базовая цена
- `PlaceableData` - только для предметов, которые ставятся или сажаются через placement систему

Пример цен:

- `Seed_Tomato`: `Price = 50`
- `Seed_Carrot`: `Price = 40`
- `Produce_Tomato`: `Price = 100`
- `Produce_Carrot`: `Price = 80`

В маркете работают только предметы, которые наследуются от `SellableItem`.

## 3. Базар

Создай профиль базара:

- `Create -> Market -> Bazaar Profile`

Asset:

- `Buyer_Bazaar`

Поля:

- `BuyerName` - `Базар`
- `BuyerIcon` - иконка базара, можно оставить пустой
- `DefaultPriceMultiplier` - множитель цены продажи, например `1`

Формула продажи в базар:

- `SellPrice = Item.Price * DefaultPriceMultiplier`

Профиль `Buyer_Bazaar` назначается в `MarketManager.bazaarBuyer`.

## 4. Контрактные покупатели

Создай контрактного покупателя:

- `Create -> Market -> Contract Buyer Profile`

Asset:

- `Buyer_Restaurant`

Поля:

- `BuyerName` - `Ресторан`
- `BuyerIcon` - иконка покупателя, можно оставить пустой
- `OrderIntervalSeconds` - `60`
- `OfferLifetimeSeconds` - `180`
- `MaxOutstandingOrders` - `2`
- `MinInitialOrderMultiplier` - `1.4`
- `MaxInitialOrderMultiplier` - `1.8`
- `MultiplierDecaySeconds` - `120`
- `MultiplierStepSeconds` - `15`
- `OneXHoldSeconds` - `60`

Если ресторан должен просить только урожай, создай для него шаблоны контрактов с `Produce_Tomato`, `Produce_Carrot` и другими продуктами в `RequestedLines`.

## 5. Шаблоны контрактов

Создай шаблоны:

- `Create -> Market -> Order Template`

Примеры:

- `Order_Restaurant_Tomatoes`
- `Order_Restaurant_MixedVegetables`

Поля `OrderDataSO`:

- `Buyer` - контрактный покупатель, которому принадлежит заказ
- `RequestedLines` - строки товаров
- `RewardCoins` - минимальная награда
- `OverrideLifetime` - включает индивидуальные таймеры выполнения для этого шаблона
- `OverrideDecaySeconds` - сколько секунд коэффициент падает до `x1`
- `OverrideMultiplierStepSeconds` - раз в сколько секунд коэффициент делает следующий шаг вниз
- `OverrideOneXHoldSeconds` - сколько секунд контракт живет на `x1`

Если `Buyer` пустой, шаблон может выпасть любому контрактному покупателю из `availableBuyers`.

Пример `Order_Restaurant_MixedVegetables`:

- `Buyer = Buyer_Restaurant`
- `RequestedLines` size = `2`
- Line 0:
  - `Item = Produce_Tomato`
  - `Amount = 10`
- Line 1:
  - `Item = Produce_Carrot`
  - `Amount = 5`
- `RewardCoins = 1500`

Награда:

- Берется `RewardCoins`.
- Код считает стоимость строк по `Item.Price * RequestedAmount`.
- Используется большее значение.
- Итог умножается на текущий коэффициент контракта.

Как падает коэффициент:

- При создании контракта выбирается стартовый коэффициент между `MinInitialOrderMultiplier` и `MaxInitialOrderMultiplier` покупателя.
- После принятия контракта коэффициент падает от стартового значения до `x1` за `MultiplierDecaySeconds`.
- `MultiplierStepSeconds` задает частоту шагов падения. Например, при `MultiplierDecaySeconds = 120` и `MultiplierStepSeconds = 15` будет 8 шагов.
- После достижения `x1` контракт живет еще `OneXHoldSeconds`.
- Если у шаблона включен `OverrideLifetime`, он использует `OverrideDecaySeconds`, `OverrideMultiplierStepSeconds` и `OverrideOneXHoldSeconds` вместо этих полей покупателя.

## 6. Сценовые менеджеры

В сцене создай объект, например `GameSystems`.

На нем или на отдельных объектах должны быть:

- `PlayerWallet`
- `InventoryManager`
- `MarketManager`

### PlayerWallet

Создай объект `PlayerWallet` и добавь компонент `PlayerWallet`.

Поле:

- `startingCoins` - стартовые деньги игрока, например `1000`

### InventoryManager

Создай объект `InventoryManager` и добавь компонент `InventoryManager`.

Поля:

- `inventorySlots` - UI-слоты инвентаря
- `inventoryItemPrefab` - prefab предмета в инвентаре
- `startingItems` - стартовые предметы

Для теста добавь несколько `Produce_Tomato` и `Produce_Carrot` в `startingItems`, чтобы сразу проверить продажу и сдачу контрактов.

### MarketManager

Создай объект `MarketManager` и добавь компонент `MarketManager`.

Поля:

- `bazaarBuyer` - `Buyer_Bazaar`
- `availableBuyers` - контрактные покупатели
- `orderPool` - шаблоны контрактов
- `maxActiveOrders`
- `maxAvailableOrders`
- `bazaarSellIntervalSeconds`
- `itemDatabase` - общая база предметов игры
- `useItemDatabaseForPurchases` - брать товары для закупки из `ItemDatabaseSO`
- `includeManualPurchasableItems` - добавлять ручной список к товарам из базы
- `purchasableItems` - ручной список закупки, используется как fallback или добавка
- `buyPriceMultiplier` - множитель цены покупки

Пример:

- `bazaarBuyer = Buyer_Bazaar`
- `availableBuyers` size = `1`
  - Element 0 = `Buyer_Restaurant`
- `orderPool`
  - `Order_Restaurant_Tomatoes`
  - `Order_Restaurant_MixedVegetables`
- `maxActiveOrders = 3`
- `maxAvailableOrders = 6`
- `bazaarSellIntervalSeconds = 90`
- `itemDatabase = ItemDatabase`
- `useItemDatabaseForPurchases = true`
- `includeManualPurchasableItems = false`
- `purchasableItems` можно оставить пустым
- `buyPriceMultiplier = 1`

`availableBuyers` должен содержать только `Contract Buyer Profile`.

При `useItemDatabaseForPurchases = true` базарная закупка автоматически берет из `itemDatabase.Items` все предметы типа `SeedItem`. Продажа не использует базу напрямую: она строится из sellable-предметов, которые есть в инвентаре игрока.

## 7. Canvas маркета

В Canvas создай:

- `MarketUIRoot`
- `MarketPanel`
- `Left_Mart`
- `Right_Orders`

Иерархия может быть такой:

```text
MarketUIRoot
└─ MarketPanel
   ├─ Left_Mart
   │  ├─ BuyTitle
   │  ├─ BuyListParent
   │  ├─ BuyTotalText
   │  ├─ SellTitle
   │  ├─ SellListParent
   │  ├─ SellTotalText
   │  ├─ NetTotalText
   │  ├─ DispatchTimerText
   │  └─ DispatchButton
   └─ Right_Orders
      ├─ AvailableTitle
      ├─ AvailableOrdersParent
      ├─ ActiveTitle
      └─ ActiveOrdersParent
```

`Left_Mart` и `Right_Orders` - обычные UI-объекты с `RectTransform`. Для них удобно добавить `Vertical Layout Group`.

Для контейнеров карточек создай пустые UI-объекты:

- `BuyListParent`
- `SellListParent`
- `AvailableOrdersParent`
- `ActiveOrdersParent`

На эти контейнеры удобно добавить:

- `Vertical Layout Group`
- `Content Size Fitter`, если контейнер должен подстраиваться по высоте

Текстовые объекты создай через:

- `UI -> Text - TextMeshPro`

Кнопку отправки создай через:

- `UI -> Button - TextMeshPro`

На `MarketUIRoot` добавь `MarketUIController`.

Привязки `MarketUIController`:

- `marketPanel` -> `MarketPanel`
- `bazaarContent` -> `Left_Mart`
- `ordersContent` -> `Right_Orders`
- `buyListParent` -> `BuyListParent`
- `sellListParent` -> `SellListParent`
- `marketSlotPrefab` -> `MarketSlotPrefab`
- `buyTotalText` -> `BuyTotalText`
- `sellTotalText` -> `SellTotalText`
- `netTotalText` -> `NetTotalText`
- `dispatchTimerText` -> `DispatchTimerText`
- `dispatchButton` -> `DispatchButton`
- `availableOrdersParent` -> `AvailableOrdersParent`
- `activeOrdersParent` -> `ActiveOrdersParent`
- `orderCardPrefab` -> `OrderCardPrefab`
- `coinsText` -> текст денег игрока, если хочешь показывать его в панели
- `buyerDropdown` -> опционально

UI показывает базар и контракты одновременно.

## 8. MarketSlotPrefab

Создай UI prefab `MarketSlotPrefab`.

На корень повесь `MarketSlotUI`.

Элементы:

- `ItemIcon` - `Image`
- `ItemNameText` - `TMP_Text`
- `PriceText` - `TMP_Text`
- `UnitPriceText` - `TMP_Text`
- `QuantityInput` - `TMP_InputField`
- `MaxButton` - `Button`

Привязки:

- `itemIcon` -> `ItemIcon`
- `itemNameText` -> `ItemNameText`
- `priceText` -> `PriceText`
- `unitPriceText` -> `UnitPriceText`
- `quantityInput` -> `QuantityInput`
- `maxButton` -> `MaxButton`

Поведение:

- Закупка строится из `ItemDatabaseSO`: в список попадают все `SeedItem`.
- Продажа строится из sellable-предметов в инвентаре.
- `PriceText` показывает сумму по карточке.
- `UnitPriceText` показывает цену за 1 штуку.
- `QuantityInput` обновляет итог сделки.
- `MaxButton` показывается только при продаже.

## 9. OrderLinePrefab

Создай UI prefab `OrderLinePrefab`.

На корень повесь `OrderLineUI`.

Элементы:

- `ItemIcon` - `Image`
- `AmountText` - `TMP_Text`
- `PriceText` - `TMP_Text`
- `DeliverButton` - `Button`
- `DeliverButtonText` - `TMP_Text`

Привязки:

- `itemIcon` -> `ItemIcon`
- `amountText` -> `AmountText`
- `priceText` -> `PriceText`
- `deliverButton` -> `DeliverButton`
- `deliverButtonText` -> `DeliverButtonText`

Поведение:

- В доступном контракте строка показывает `xN`.
- В принятом контракте строка показывает `m/n`.
- `DeliverButton` виден только в принятом контракте.

## 10. OrderCardPrefab

Создай UI prefab `OrderCardPrefab`.

На корень повесь `OrderCardUI`.

Элементы:

- `BuyerIcon` - `Image`
- `BuyerNameText` - `TMP_Text`
- `RewardText` - `TMP_Text`
- `MultiplierText` - `TMP_Text`
- `LifeTimerText` - `TMP_Text`
- `NextDropTimerText` - `TMP_Text`
- `LinesSummaryIconsParent` - контейнер маленьких иконок товаров контракта
- `ExpandButton` - `Button`
- `LinesDetailsRoot` - объект раскрытия
- `LinesDetailsParent` - контейнер строк
- `AcceptButton` - `Button`
- `RejectButton` - `Button`
- `DeliverAllButton` - `Button`

Привязки:

- `buyerIcon` -> `BuyerIcon`
- `buyerNameText` -> `BuyerNameText`
- `rewardText` -> `RewardText`
- `multiplierText` -> `MultiplierText`
- `lifeTimerText` -> `LifeTimerText`
- `nextDropTimerText` -> `NextDropTimerText`
- `linesSummaryIconsParent` -> `LinesSummaryIconsParent`
- `expandButton` -> `ExpandButton`
- `linesDetailsRoot` -> `LinesDetailsRoot`
- `linesDetailsParent` -> `LinesDetailsParent`
- `orderLinePrefab` -> `OrderLinePrefab`
- `acceptButton` -> `AcceptButton`
- `rejectButton` -> `RejectButton`
- `deliverAllButton` -> `DeliverAllButton`

Опционально:

- `cardBackground`
- `availableBackgroundColor`
- `activeBackgroundColor`

`LinesSummaryIconsParent` лучше сделать горизонтальным контейнером:

- `RectTransform`
- `Horizontal Layout Group`

Код сам создаст в нем маленькие `Image` с иконками всех товаров контракта. Для контракта с одним товаром будет одна иконка, для контракта с несколькими товарами - несколько иконок, как на макете.

## 11. Здание рынка

В сцене создай объект рынка, например `MarketBuilding`.

Компоненты:

- `Box Collider 2D`
- `MarketBuilding`

Поля `MarketBuilding`:

- `marketUI` -> объект с `MarketUIController`
- `interactionDistance` -> например `5`
- `playerTransform` -> Transform игрока

`MarketBuilding` использует `OnMouseDown`, поэтому объекту нужен `Collider2D`, а камера должна видеть этот объект.

## 12. Gameplay loop

Покупка в базаре:

1. Игрок вводит количество в карточке закупки.
2. `MarketManager` считает сумму.
3. При нажатии `DispatchButton` деньги списываются через `PlayerWallet.SpendCoins`.
4. После таймера купленные товары добавляются в `InventoryManager`.

Продажа в базаре:

1. Игрок вводит количество в карточке продажи.
2. При отправке товары удаляются из `InventoryManager`.
3. После таймера деньги начисляются через `PlayerWallet.AddCoins`.

Контракт:

1. Игрок принимает контракт.
2. Контракт переходит из available в active.
3. Игрок сдает товары через `DeliverAllButton` или кнопки строк.
4. Товары удаляются из `InventoryManager`.
5. Когда все строки закрыты, награда начисляется в `PlayerWallet`.

## 13. Минимальный тест

Items:

- `Seed_Tomato`, price `50`
- `Seed_Carrot`, price `40`
- `Produce_Tomato`, price `100`
- `Produce_Carrot`, price `80`

Bazaar:

- `Buyer_Bazaar` как `Bazaar Profile`
- `DefaultPriceMultiplier = 1`

Contract Buyers:

- `Buyer_Restaurant` как `Contract Buyer Profile`
- `OrderIntervalSeconds = 60`
- `OfferLifetimeSeconds = 180`
- `MaxOutstandingOrders = 2`

Orders:

- `Order_Restaurant_Tomatoes`
  - `Buyer = Buyer_Restaurant`
  - `RequestedLines`: `Produce_Tomato`, amount `5`
  - `RewardCoins = 500`
- `Order_Restaurant_Mixed`
  - `Buyer = Buyer_Restaurant`
  - `RequestedLines`:
    - `Produce_Tomato`, amount `4`
    - `Produce_Carrot`, amount `4`
  - `RewardCoins = 800`

MarketManager:

- `bazaarBuyer`: `Buyer_Bazaar`
- `availableBuyers`: `Buyer_Restaurant`
- `orderPool`: оба order template
- `itemDatabase`: база, где есть `Seed_Tomato`, `Seed_Carrot`, `Produce_Tomato`, `Produce_Carrot`
- `useItemDatabaseForPurchases`: `true`
- `includeManualPurchasableItems`: `false`
- `bazaarSellIntervalSeconds`: `10`
- `buyPriceMultiplier`: `1`

PlayerWallet:

- `startingCoins = 1000`

InventoryManager:

- Несколько `Produce_Tomato`
- Несколько `Produce_Carrot`

## 14. Проверка в Play Mode

1. Запусти сцену.
2. Кликни по зданию рынка или вызови `MarketUIController.Open()`.
3. Убедись, что видны базар и контракты.
4. В закупке введи `2` у `Seed_Tomato`.
5. Проверь сумму карточки, `buyTotalText` и `netTotalText`.
6. В продаже введи количество урожая.
7. Нажми `DispatchButton`.
8. Проверь, что деньги за закупку списались сразу.
9. Дождись таймера.
10. Проверь, что купленные товары пришли в инвентарь, а деньги за продажу начислились.
11. Прими контракт.
12. Раскрой контракт через `ExpandButton`.
13. Нажми кнопку строки или `DeliverAllButton`.
14. Проверь прогресс `m/n`.
15. После заполнения всех строк проверь начисление награды.

## 15. Частые проблемы

Карточки закупки не появляются:

- `MarketManager` отсутствует в сцене.
- `itemDatabase` не назначен.
- В `itemDatabase.Items` нет предметов типа `SeedItem`.
- `purchasableItems` пустой, если `useItemDatabaseForPurchases` выключен.
- `marketSlotPrefab` не назначен.
- `buyListParent` не назначен.

Карточки продажи не появляются:

- `InventoryManager` отсутствует в сцене.
- В инвентаре нет предметов типа `SellableItem`.
- `sellListParent` не назначен.

Контракты не появляются:

- `orderPool` пустой.
- В `OrderDataSO` нет валидных `RequestedLines`.
- `availableBuyers` пустой.
- Шаблон привязан не к тому `Buyer`.
- `maxAvailableOrders` слишком маленький.

Кнопка отправки не активна:

- В очереди нет покупки или продажи.
- Идет таймер предыдущей отправки.
- Не хватает денег на закупку.

Контракт не закрывается:

- В инвентаре нет нужных товаров.
- Контракт истек.
- Не все строки доставлены до нужного количества.

## 16. Prefab

Обязательно сохрани как prefab:

- `MarketSlotPrefab`
- `OrderCardPrefab`
- `OrderLinePrefab`

Желательно:

- `MarketPanel`
- `MarketBuilding`

## 17. Данные маркета

Отдельной базы данных маркета нет.

`MarketManager` использует прямые поля:

- `bazaarBuyer` - профиль базара
- `availableBuyers` - контрактные покупатели
- `orderPool` - шаблоны контрактов
- `itemDatabase` - общая база предметов для автоматической закупки
- `purchasableItems` - ручной fallback/добавки к закупке

Для закупки `MarketManager` берет из `ItemDatabaseSO` все `SeedItem`. Для продажи отдельная база не нужна: список строится по инвентарю игрока.
