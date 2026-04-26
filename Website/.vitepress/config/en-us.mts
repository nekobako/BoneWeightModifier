export const main =
{
  lang: "en-us",
  label: "English",
  title: "Bone Weight Modifier",
  description: "A non-destructive tool to add and edit bone weights for VRChat avatars",
  themeConfig:
  {
    nav:
    [
      {
        text: "Home",
        link: "/",
      },
      {
        text: "Getting Started",
        link: "/getting-started/",
        activeMatch: "/getting-started/",
      },
      {
        text: "Tutorials",
        link: "/tutorials/",
        activeMatch: "/tutorials/",
      },
      {
        text: "References",
        link: "/references/",
        activeMatch: "/references/",
      },
    ],
    sidebar:
    [
      {
        text: "Getting Started",
        link: "/getting-started/",
        collapsed: false,
        items:
        [
          {
            text: "Installation",
            link: "/getting-started/installation",
          },
        ],
      },
      {
        text: "Tutorials",
        link: "/tutorials/",
        collapsed: false,
        items:
        [
          {
            text: "Soft, Squishy Nose",
            link: "/tutorials/soft-squishy-nose",
          },
          {
            text: "Soft, Squishy Cheeks",
            link: "/tutorials/soft-squishy-cheeks",
          },
          {
            text: "Soft, Squishy Cube",
            link: "/tutorials/soft-squishy-cube",
          },
          {
            text: "Separate Accessories",
            link: "/tutorials/separate-accessories",
          },
        ],
      },
      {
        text: "References",
        link: "/references/",
        collapsed: false,
        items:
        [
          {
            text: "Bone Weight Modifier Component",
            link: "/references/bone-weight-modifier-component",
          },
          {
            text: "Weights",
            link: "/references/weights/",
            collapsed: false,
            items:
            [
              {
                text: "Volume Weight",
                link: "/references/weights/volume-weight",
              },
              {
                text: "Mask Weight",
                link: "/references/weights/mask-weight",
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
      buttonText: "Search",
    },
    modal:
    {
      displayDetails: "Display details",
      resetButtonTitle: "Reset search",
      backButtonTitle: "Close search",
      noResultsText: "No results",
      footer:
      {
        navigateText: "Navigate",
        selectText: "Select",
        closeText: "Close",
      },
    },
  },
};
