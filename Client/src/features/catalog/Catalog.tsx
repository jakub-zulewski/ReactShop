import { Product } from "../../app/models/product";

interface Props {
  products: Product[];
}

export default function Catalog({ products }: Props) {
  return (
    <>
      <h1>Catalog</h1>
      <ul>
        {products.map((product) => (
          <li key={product.id}>
            {product.name} - {product.price}
          </li>
        ))}
      </ul>
    </>
  );
}
