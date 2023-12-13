import { useState, useEffect } from "react";
import { Product } from "../../app/models/product";
import ProductList from "./ProductList";
import agent from "../../app/api/agent";
import { router } from "../../app/router/Routes";
import { toast } from "react-toastify";

export default function Catalog() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    agent.Catalog.list()
      .then((products) => setProducts(products))
      .catch(() => {
        toast.error("Something went wrong.");
        router.navigate("/");
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <h3>Loading...</h3>;

  return <ProductList products={products} />;
}
