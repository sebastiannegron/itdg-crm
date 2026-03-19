import { render, screen, fireEvent, within } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { DataTable, type DataTableColumn } from "@/app/_components/DataTable";

interface TestRow {
  id: number;
  name: string;
  email: string;
}

const columns: DataTableColumn<TestRow>[] = [
  { key: "id", header: "ID", sortable: true },
  { key: "name", header: "Name", sortable: true },
  { key: "email", header: "Email" },
];

const data: TestRow[] = [
  { id: 1, name: "Alice", email: "alice@example.com" },
  { id: 2, name: "Bob", email: "bob@example.com" },
  { id: 3, name: "Charlie", email: "charlie@example.com" },
];

function getDesktopTable() {
  // The desktop table is inside a div that is hidden on mobile (hidden md:block)
  return document.querySelector(".hidden.md\\:block") as HTMLElement;
}

describe("DataTable", () => {
  it("renders table headers in desktop view", () => {
    render(<DataTable columns={columns} data={data} />);
    const desktop = getDesktopTable();
    expect(desktop).toBeInTheDocument();
    expect(within(desktop).getByText("ID")).toBeInTheDocument();
    expect(within(desktop).getByText("Name")).toBeInTheDocument();
    expect(within(desktop).getByText("Email")).toBeInTheDocument();
  });

  it("renders data rows in desktop view", () => {
    render(<DataTable columns={columns} data={data} />);
    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Alice")).toBeInTheDocument();
    expect(
      within(desktop).getByText("bob@example.com")
    ).toBeInTheDocument();
  });

  it("renders empty message when no data", () => {
    render(<DataTable columns={columns} data={[]} />);
    const desktop = getDesktopTable();
    expect(
      within(desktop).getByText("No results found.")
    ).toBeInTheDocument();
  });

  it("renders custom empty message", () => {
    render(
      <DataTable
        columns={columns}
        data={[]}
        emptyMessage="No clients found."
      />
    );
    const desktop = getDesktopTable();
    expect(
      within(desktop).getByText("No clients found.")
    ).toBeInTheDocument();
  });

  it("sorts data ascending when sortable header is clicked", () => {
    render(<DataTable columns={columns} data={data} />);
    const desktop = getDesktopTable();

    const sortButton = within(desktop).getByLabelText("Sort by Name");
    fireEvent.click(sortButton);

    const cells = within(desktop).getAllByRole("cell");
    const nameCells = cells.filter((_, index) => index % 3 === 1);
    expect(nameCells[0]).toHaveTextContent("Alice");
    expect(nameCells[1]).toHaveTextContent("Bob");
    expect(nameCells[2]).toHaveTextContent("Charlie");
  });

  it("sorts descending on second click", () => {
    render(<DataTable columns={columns} data={data} />);
    const desktop = getDesktopTable();

    const sortButton = within(desktop).getByLabelText("Sort by Name");
    fireEvent.click(sortButton); // asc
    fireEvent.click(sortButton); // desc

    const cells = within(desktop).getAllByRole("cell");
    const nameCells = cells.filter((_, index) => index % 3 === 1);
    expect(nameCells[0]).toHaveTextContent("Charlie");
    expect(nameCells[1]).toHaveTextContent("Bob");
    expect(nameCells[2]).toHaveTextContent("Alice");
  });

  it("filters data when search is used", () => {
    render(<DataTable columns={columns} data={data} searchable />);

    const searchInput = screen.getByPlaceholderText("Search...");
    fireEvent.change(searchInput, { target: { value: "alice" } });

    const desktop = getDesktopTable();
    expect(within(desktop).getByText("Alice")).toBeInTheDocument();
    expect(within(desktop).queryByText("Bob")).not.toBeInTheDocument();
  });

  it("paginates data", () => {
    const manyRows: TestRow[] = Array.from({ length: 15 }, (_, i) => ({
      id: i + 1,
      name: `User ${i + 1}`,
      email: `user${i + 1}@example.com`,
    }));

    render(<DataTable columns={columns} data={manyRows} pageSize={10} />);
    const desktop = getDesktopTable();

    expect(within(desktop).getByText("User 1")).toBeInTheDocument();
    expect(within(desktop).getByText("User 10")).toBeInTheDocument();
    expect(
      within(desktop).queryByText("User 11")
    ).not.toBeInTheDocument();

    const nextButton = screen.getByLabelText("Next page");
    fireEvent.click(nextButton);

    expect(within(desktop).getByText("User 11")).toBeInTheDocument();
    expect(
      within(desktop).queryByText("User 1")
    ).not.toBeInTheDocument();
  });

  it("renders mobile card view", () => {
    render(<DataTable columns={columns} data={data} />);
    const mobileContainer = document.querySelector(
      ".md\\:hidden"
    ) as HTMLElement;
    expect(mobileContainer).toBeInTheDocument();
  });

  it("renders mobile card view with custom renderCard", () => {
    render(
      <DataTable
        columns={columns}
        data={data}
        renderCard={(row) => (
          <div data-testid="custom-card">{row.name}</div>
        )}
      />
    );

    const customCards = screen.getAllByTestId("custom-card");
    expect(customCards.length).toBe(3);
  });
});
