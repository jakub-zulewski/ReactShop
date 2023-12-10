import { Avatar, List, ListItem, ListItemText } from "@mui/material";
import { Product } from "../../app/models/product";

interface Props {
  products: Product[];
}

export default function Catalog({ products }: Props) {
  return (
    <List>
      {products.map((product) => (
        <ListItem key={product.id}>
          <Avatar src={product.pictureUrl} />
          <ListItemText>
            {product.name} - {product.price}
          </ListItemText>
        </ListItem>
      ))}
    </List>
  );
}
