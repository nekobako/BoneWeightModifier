export const main =
{
  lang: "ja-jp",
  label: "日本語",
  title: "ボーンウェイトモディファイヤー",
  description: "VRChat アバターのための非破壊ボーンウェイト追加/編集ツール",
  themeConfig:
  {
    nav:
    [
      {
        text: "ホーム",
        link: "/ja-jp/",
      },
      {
        text: "はじめに",
        link: "/ja-jp/getting-started/",
        activeMatch: "/ja-jp/getting-started/",
      },
      {
        text: "チュートリアル",
        link: "/ja-jp/tutorials/",
        activeMatch: "/ja-jp/tutorials/",
      },
      {
        text: "リファレンス",
        link: "/ja-jp/references/",
        activeMatch: "/ja-jp/references/",
      },
    ],
    sidebar:
    [
      {
        text: "はじめに",
        link: "/ja-jp/getting-started/",
        collapsed: false,
        items:
        [
          {
            text: "インストール",
            link: "/ja-jp/getting-started/installation",
          },
        ],
      },
      {
        text: "チュートリアル",
        link: "/ja-jp/tutorials/",
        collapsed: false,
        items:
        [
          {
            text: "ふにふにのおはな",
            link: "/ja-jp/tutorials/soft-squishy-nose",
          },
          {
            text: "もちもちのほっぺ",
            link: "/ja-jp/tutorials/soft-squishy-cheeks",
          },
          {
            text: "ぷるぷるのキューブ",
            link: "/ja-jp/tutorials/soft-squishy-cube",
          },
          {
            text: "アクセサリの分離",
            link: "/ja-jp/tutorials/separate-accessories",
          },
        ],
      },
      {
        text: "リファレンス",
        link: "/ja-jp/references/",
        collapsed: false,
        items:
        [
          {
            text: "Bone Weight Modifier コンポーネント",
            link: "/ja-jp/references/bone-weight-modifier-component",
          },
          {
            text: "ウェイト",
            link: "/ja-jp/references/weights/",
            collapsed: false,
            items:
            [
              {
                text: "Volume ウェイト",
                link: "/ja-jp/references/weights/volume-weight",
              },
              {
                text: "Mask ウェイト",
                link: "/ja-jp/references/weights/mask-weight",
              },
            ],
          },
        ],
      },
    ],
  },
};

export const search =
{
  translations:
  {
    button:
    {
      buttonText: "検索",
    },
    modal:
    {
      displayDetails: "詳細を表示",
      resetButtonTitle: "検索をリセット",
      backButtonTitle: "検索を閉じる",
      noResultsText: "結果がありません",
      footer:
      {
        navigateText: "移動",
        selectText: "選択",
        closeText: "閉じる",
      },
    },
  },
};
