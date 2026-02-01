using UnityEngine;

namespace Generator.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        public event System.Action Clicked;

        public void SetColor(Color color)
        {
            _renderer.material.color = color;
        }

        void OnMouseDown()
        {
            Clicked?.Invoke();
        }
    }
}
