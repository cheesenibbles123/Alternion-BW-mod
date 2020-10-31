using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Alternion
{
    class LoadingBar : MonoBehaviour
    {
        public Image cooldown;
        public float filledValue = 100.0f;

        void Start()
        {
            cooldown.color = new Color32(255, 255, 225, 100);
            cooldown.type = Image.Type.Filled;
            cooldown.fillAmount = 0.0f;
            cooldown.fillMethod = Image.FillMethod.Horizontal;
            cooldown.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        public void setPercentage(float percentage)
        {
            cooldown.fillAmount = filledValue * (percentage / 100);
        }
    }
}
